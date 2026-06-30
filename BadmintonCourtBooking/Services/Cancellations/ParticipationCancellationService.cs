using System.Data;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.Participations;
using BadmintonCourtBooking.Dtos.Wallet;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class ParticipationCancellationService(
    ApplicationDbContext dbContext,
    IClock clock,
    ICancellationPolicy cancellationPolicy,
    IWalletAccountingService walletAccountingService,
    INotificationService notificationService) : IParticipationCancellationService
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

        var guestWallet = await walletAccountingService.GetWalletAsync(participant.UserId, cancellationToken);
        if (guestWallet is null || guestWallet.HeldBalanceVnd < payment.AmountVnd)
            return ServiceResult<CancellationResponse>.Failure("INSUFFICIENT_HELD_BALANCE", "Held balance is not enough to cancel.");

        var hostWallet = await walletAccountingService.GetOrCreateWalletAsync(participant.PlaySessionPost.CreatorUserId, cancellationToken);
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
        walletAccountingService.ApplyParticipantCancellation(
            guestWallet,
            hostWallet,
            request.RefundChoice,
            quote,
            now,
            participant.PlaySessionPost.CreatorUserId,
            participant.UserId,
            participant.PlaySessionPostId,
            participant.JoinRequestId,
            cancellation.Id);

        participant.Cancel(now);
        participant.JoinRequest.Cancel(now);

        notificationService.Add(
            participant.UserId,
            request.RefundChoice == CancellationRefundChoice.StandardRefund
                ? NotificationType.RefundCompleted
                : NotificationType.NoRefundCancellation,
            "Participation cancelled",
            request.RefundChoice == CancellationRefundChoice.StandardRefund
                ? $"Your cancellation was completed. Refund: {quote.RefundAmountVnd} VND."
                : "Your cancellation was completed without a refund.",
            now,
            cancellation.Id.ToString());

        notificationService.Add(
            participant.PlaySessionPost.CreatorUserId,
            NotificationType.ParticipantCancelled,
            "Participant cancelled",
            $"{participant.UserId} cancelled participation in {participant.PlaySessionPost.Title}.",
            now,
            cancellation.Id.ToString());

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
}
