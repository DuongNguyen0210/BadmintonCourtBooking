using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IWalletAccountingService
{
    Task<Wallet?> GetWalletAsync(string userId, CancellationToken cancellationToken);

    Task<Wallet> GetOrCreateWalletAsync(string userId, CancellationToken cancellationToken);

    void TopUp(
        Wallet wallet,
        long amountVnd,
        DateTimeOffset now,
        string idempotencyKey);

    void HoldEscrow(
        Wallet wallet,
        long amountVnd,
        DateTimeOffset now,
        string relatedUserId,
        Guid playSessionPostId,
        Guid joinRequestId,
        string description,
        string idempotencyKey);

    void ApplyParticipantCancellation(
        Wallet guestWallet,
        Wallet hostWallet,
        CancellationRefundChoice refundChoice,
        CancellationQuote quote,
        DateTimeOffset now,
        string hostUserId,
        string guestUserId,
        Guid playSessionPostId,
        Guid joinRequestId,
        Guid cancellationId);

    void ApplyHostCancellationFullRefund(
        Wallet guestWallet,
        long amountVnd,
        DateTimeOffset now,
        string hostUserId,
        Guid playSessionPostId,
        Guid joinRequestId);
}
