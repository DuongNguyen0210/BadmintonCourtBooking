using System.Data;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.Participations;
using BadmintonCourtBooking.Dtos.Wallet;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class CancellationService(
    ApplicationDbContext dbContext,
    IClock clock,
    ICancellationPolicy cancellationPolicy) : ICancellationService
{
    private const string WaiveRefundConfirmationText = "KHONG HOAN TIEN";

    public async Task<ServiceResult<CancellationResponse>> CancelParticipationAsync(
        Guid participantId,
        string userId,
        CancelParticipationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.RefundChoice == CancellationRefundChoice.WaiveRefund &&
            request.WaiveRefundConfirmation != WaiveRefundConfirmationText)
        {
            return ServiceResult<CancellationResponse>.Failure(
                "WAIVE_REFUND_CONFIRMATION_REQUIRED",
                "Waive refund cancellation requires the exact confirmation text.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var participant = await dbContext.PlaySessionParticipants
            .Include(existingParticipant => existingParticipant.PlaySessionPost)
            .Include(existingParticipant => existingParticipant.JoinRequest)
            .FirstOrDefaultAsync(existingParticipant => existingParticipant.Id == participantId, cancellationToken);

        if (participant is null)
            return ServiceResult<CancellationResponse>.Failure("PARTICIPANT_NOT_FOUND", "Participant was not found.");

        if (participant.UserId != userId)
            return ServiceResult<CancellationResponse>.Failure("FORBIDDEN", "You can only cancel your own participation.");

        var now = clock.UtcNow;
        var validationError = await ValidateParticipantCancellationAsync(participant, now, cancellationToken);
        if (validationError is not null)
            return ServiceResult<CancellationResponse>.Failure(validationError.Code, validationError.Message);

        var payment = await GetOriginalPaymentAsync(participant.JoinRequestId, cancellationToken);
        if (payment is null)
            return ServiceResult<CancellationResponse>.Failure("PAYMENT_NOT_FOUND", "Original payment was not found.");

        var guestWallet = await GetWalletAsync(participant.UserId, cancellationToken);
        if (guestWallet is null || guestWallet.HeldBalanceVnd < payment.AmountVnd)
            return ServiceResult<CancellationResponse>.Failure("INSUFFICIENT_HELD_BALANCE", "Held balance is not enough to cancel.");

        var hostWallet = await GetOrCreateWalletAsync(participant.PlaySessionPost.CreatorUserId, cancellationToken);
        var quote = request.RefundChoice == CancellationRefundChoice.StandardRefund
            ? cancellationPolicy.Quote(payment.AmountVnd)
            : new CancellationQuote(payment.AmountVnd, 0, payment.AmountVnd);

        var cancellation = ParticipationCancellation.Complete(
            participant.Id,
            userId,
            request.RefundChoice,
            quote.OriginalAmountVnd,
            quote.RefundAmountVnd,
            quote.CancellationFeeVnd,
            now,
            request.Reason);

        dbContext.ParticipationCancellations.Add(cancellation);

        var guestAvailableBefore = guestWallet.AvailableBalanceVnd;
        guestWallet.ReleaseHeld(payment.AmountVnd, now);

        if (quote.RefundAmountVnd > 0)
        {
            guestWallet.CreditAvailable(quote.RefundAmountVnd, now);
            dbContext.WalletTransactions.Add(WalletTransaction.CreateCompleted(
                guestWallet.UserId,
                WalletTransactionType.Refund,
                quote.RefundAmountVnd,
                guestAvailableBefore,
                guestWallet.AvailableBalanceVnd,
                "Participation cancellation refund",
                now,
                relatedUserId: participant.PlaySessionPost.CreatorUserId,
                playSessionPostId: participant.PlaySessionPostId,
                joinRequestId: participant.JoinRequestId,
                cancellationId: cancellation.Id));
        }

        var hostAvailableBefore = hostWallet.AvailableBalanceVnd;
        hostWallet.CreditAvailable(quote.CancellationFeeVnd, now);
        dbContext.WalletTransactions.Add(WalletTransaction.CreateCompleted(
            hostWallet.UserId,
            request.RefundChoice == CancellationRefundChoice.StandardRefund
                ? WalletTransactionType.CancellationFee
                : WalletTransactionType.EscrowReleaseToHost,
            quote.CancellationFeeVnd,
            hostAvailableBefore,
            hostWallet.AvailableBalanceVnd,
            request.RefundChoice == CancellationRefundChoice.StandardRefund
                ? "Participation cancellation fee"
                : "Participation cancellation without refund",
            now,
            relatedUserId: participant.UserId,
            playSessionPostId: participant.PlaySessionPostId,
            joinRequestId: participant.JoinRequestId,
            cancellationId: cancellation.Id));

        participant.Cancel(now);
        participant.JoinRequest.Cancel(now);

        dbContext.Notifications.Add(Notification.Create(
            participant.UserId,
            request.RefundChoice == CancellationRefundChoice.StandardRefund
                ? NotificationType.RefundCompleted
                : NotificationType.NoRefundCancellation,
            "Participation cancelled",
            request.RefundChoice == CancellationRefundChoice.StandardRefund
                ? $"Your cancellation was completed. Refund: {quote.RefundAmountVnd} VND."
                : "Your cancellation was completed without a refund.",
            now,
            cancellation.Id.ToString()));

        dbContext.Notifications.Add(Notification.Create(
            participant.PlaySessionPost.CreatorUserId,
            NotificationType.ParticipantCancelled,
            "Participant cancelled",
            $"{participant.UserId} cancelled participation in {participant.PlaySessionPost.Title}.",
            now,
            cancellation.Id.ToString()));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<CancellationResponse>.Success(new CancellationResponse(
            cancellation.Id,
            participant.Id,
            quote.OriginalAmountVnd,
            quote.RefundAmountVnd,
            quote.CancellationFeeVnd,
            request.RefundChoice.ToString(),
            new WalletResponse(guestWallet.AvailableBalanceVnd, guestWallet.HeldBalanceVnd)));
    }

    public async Task<ServiceResult<object>> CancelPlaySessionByHostAsync(
        Guid playSessionPostId,
        string hostUserId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var post = await dbContext.PlaySessionPosts.FirstOrDefaultAsync(
            playSessionPost => playSessionPost.Id == playSessionPostId,
            cancellationToken);

        if (post is null)
            return ServiceResult<object>.Failure("PLAY_SESSION_NOT_FOUND", "Play session was not found.");

        if (post.CreatorUserId != hostUserId)
            return ServiceResult<object>.Failure("FORBIDDEN", "Only the host can cancel this play session.");

        if (post.Status == PostStatus.Cancelled)
            return ServiceResult<object>.Success(new { });

        var now = clock.UtcNow;
        var participants = await dbContext.PlaySessionParticipants
            .Include(participant => participant.JoinRequest)
            .Where(participant =>
                participant.PlaySessionPostId == playSessionPostId &&
                participant.Status == ParticipantStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var participant in participants)
        {
            var payment = await GetOriginalPaymentAsync(participant.JoinRequestId, cancellationToken);
            if (payment is null)
                return ServiceResult<object>.Failure("PAYMENT_NOT_FOUND", "Original payment was not found.");

            var guestWallet = await GetWalletAsync(participant.UserId, cancellationToken);
            if (guestWallet is null || guestWallet.HeldBalanceVnd < payment.AmountVnd)
                return ServiceResult<object>.Failure("INSUFFICIENT_HELD_BALANCE", "Held balance is not enough to refund.");

            var balanceBefore = guestWallet.AvailableBalanceVnd;
            guestWallet.ReleaseHeld(payment.AmountVnd, now);
            guestWallet.CreditAvailable(payment.AmountVnd, now);

            dbContext.WalletTransactions.Add(WalletTransaction.CreateCompleted(
                participant.UserId,
                WalletTransactionType.FullRefund,
                payment.AmountVnd,
                balanceBefore,
                guestWallet.AvailableBalanceVnd,
                "Host cancelled play session full refund",
                now,
                relatedUserId: hostUserId,
                playSessionPostId: playSessionPostId,
                joinRequestId: participant.JoinRequestId));

            participant.Cancel(now);
            participant.JoinRequest.Cancel(now);

            dbContext.Notifications.Add(Notification.Create(
                participant.UserId,
                NotificationType.HostCancelledSession,
                "Play session cancelled",
                $"Host cancelled {post.Title}. Your payment was fully refunded.",
                now,
                playSessionPostId.ToString()));
        }

        var pendingRequests = await dbContext.PlaySessionJoinRequests
            .Where(joinRequest =>
                joinRequest.PlaySessionPostId == playSessionPostId &&
                (joinRequest.Status == JoinRequestStatus.PendingHostApproval ||
                 joinRequest.Status == JoinRequestStatus.AwaitingPayment))
            .ToListAsync(cancellationToken);

        foreach (var request in pendingRequests)
            request.Cancel(now);

        post.Status = PostStatus.Cancelled;
        post.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<object>.Success(new { });
    }

    private async Task<ServiceError?> ValidateParticipantCancellationAsync(
        PlaySessionParticipant participant,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (participant.Status != ParticipantStatus.Active)
            return new ServiceError("PARTICIPANT_ALREADY_CANCELLED", "Participant is already cancelled.");

        if (participant.PlaySessionPost.Status is not (PostStatus.Active or PostStatus.Filled))
            return new ServiceError("PLAY_SESSION_NOT_ACTIVE", "Play session is not active.");

        if (participant.PlaySessionPost.StartTime <= now)
            return new ServiceError("PLAY_SESSION_ALREADY_STARTED", "Cannot cancel after the play session starts.");

        var hasCancellation = await dbContext.ParticipationCancellations.AnyAsync(
            cancellation => cancellation.ParticipantId == participant.Id,
            cancellationToken);

        return hasCancellation
            ? new ServiceError("PARTICIPATION_ALREADY_CANCELLED", "Participation was already cancelled.")
            : null;
    }

    private Task<WalletTransaction?> GetOriginalPaymentAsync(Guid joinRequestId, CancellationToken cancellationToken)
    {
        return dbContext.WalletTransactions.FirstOrDefaultAsync(
            transaction =>
                transaction.JoinRequestId == joinRequestId &&
                transaction.Type == WalletTransactionType.EscrowHold &&
                transaction.Status == WalletTransactionStatus.Completed,
            cancellationToken);
    }

    private Task<Wallet?> GetWalletAsync(string userId, CancellationToken cancellationToken)
    {
        return dbContext.Wallets.SingleOrDefaultAsync(wallet => wallet.UserId == userId, cancellationToken);
    }

    private async Task<Wallet> GetOrCreateWalletAsync(string userId, CancellationToken cancellationToken)
    {
        var wallet = await GetWalletAsync(userId, cancellationToken);
        if (wallet is not null)
            return wallet;

        wallet = Wallet.Create(userId, clock.UtcNow);
        dbContext.Wallets.Add(wallet);

        return wallet;
    }
}
