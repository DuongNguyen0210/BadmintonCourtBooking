using System.Data;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class HostPlaySessionCancellationService(
    ApplicationDbContext dbContext,
    IClock clock,
    IWalletAccountingService walletAccountingService,
    INotificationService notificationService) : IHostPlaySessionCancellationService
{
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

            var guestWallet = await walletAccountingService.GetWalletAsync(participant.UserId, cancellationToken);
            if (guestWallet is null || guestWallet.HeldBalanceVnd < payment.AmountVnd)
                return ServiceResult<object>.Failure("INSUFFICIENT_HELD_BALANCE", "Held balance is not enough to refund.");

            walletAccountingService.ApplyHostCancellationFullRefund(
                guestWallet,
                payment.AmountVnd,
                now,
                hostUserId,
                playSessionPostId,
                participant.JoinRequestId);

            participant.Cancel(now);
            participant.JoinRequest.Cancel(now);

            notificationService.Add(
                participant.UserId,
                NotificationType.HostCancelledSession,
                "Play session cancelled",
                $"Host cancelled {post.Title}. Your payment was fully refunded.",
                now,
                playSessionPostId.ToString());
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
