using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class WalletAccountingService(
    ApplicationDbContext dbContext,
    IClock clock) : IWalletAccountingService
{
    public Task<Wallet?> GetWalletAsync(string userId, CancellationToken cancellationToken)
    {
        return dbContext.Wallets.SingleOrDefaultAsync(wallet => wallet.UserId == userId, cancellationToken);
    }

    public async Task<Wallet> GetOrCreateWalletAsync(string userId, CancellationToken cancellationToken)
    {
        var wallet = await GetWalletAsync(userId, cancellationToken);
        if (wallet is not null)
            return wallet;

        wallet = Wallet.Create(userId, clock.UtcNow);
        dbContext.Wallets.Add(wallet);

        return wallet;
    }

    public void TopUp(
        Wallet wallet,
        long amountVnd,
        DateTimeOffset now,
        string idempotencyKey)
    {
        var balanceBefore = wallet.AvailableBalanceVnd;
        wallet.CreditAvailable(amountVnd, now);

        dbContext.WalletTransactions.Add(WalletTransaction.CreateCompleted(
            wallet.UserId,
            WalletTransactionType.TopUp,
            amountVnd,
            balanceBefore,
            wallet.AvailableBalanceVnd,
            "Development wallet top-up",
            now,
            idempotencyKey: idempotencyKey));
    }

    public void HoldEscrow(
        Wallet wallet,
        long amountVnd,
        DateTimeOffset now,
        string relatedUserId,
        Guid playSessionPostId,
        Guid joinRequestId,
        string description,
        string idempotencyKey)
    {
        var balanceBefore = wallet.AvailableBalanceVnd;
        wallet.MoveAvailableToHeld(amountVnd, now);

        dbContext.WalletTransactions.Add(WalletTransaction.CreateCompleted(
            wallet.UserId,
            WalletTransactionType.EscrowHold,
            amountVnd,
            balanceBefore,
            wallet.AvailableBalanceVnd,
            description,
            now,
            relatedUserId: relatedUserId,
            playSessionPostId: playSessionPostId,
            joinRequestId: joinRequestId,
            idempotencyKey: idempotencyKey));
    }

    public void ApplyParticipantCancellation(
        Wallet guestWallet,
        Wallet hostWallet,
        CancellationRefundChoice refundChoice,
        CancellationQuote quote,
        DateTimeOffset now,
        string hostUserId,
        string guestUserId,
        Guid playSessionPostId,
        Guid joinRequestId,
        Guid cancellationId)
    {
        var guestAvailableBefore = guestWallet.AvailableBalanceVnd;
        guestWallet.ReleaseHeld(quote.OriginalAmountVnd, now);

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
                relatedUserId: hostUserId,
                playSessionPostId: playSessionPostId,
                joinRequestId: joinRequestId,
                cancellationId: cancellationId));
        }

        var hostAvailableBefore = hostWallet.AvailableBalanceVnd;
        hostWallet.CreditAvailable(quote.CancellationFeeVnd, now);

        dbContext.WalletTransactions.Add(WalletTransaction.CreateCompleted(
            hostWallet.UserId,
            refundChoice == CancellationRefundChoice.StandardRefund
                ? WalletTransactionType.CancellationFee
                : WalletTransactionType.EscrowReleaseToHost,
            quote.CancellationFeeVnd,
            hostAvailableBefore,
            hostWallet.AvailableBalanceVnd,
            refundChoice == CancellationRefundChoice.StandardRefund
                ? "Participation cancellation fee"
                : "Participation cancellation without refund",
            now,
            relatedUserId: guestUserId,
            playSessionPostId: playSessionPostId,
            joinRequestId: joinRequestId,
            cancellationId: cancellationId));
    }

    public void ApplyHostCancellationFullRefund(
        Wallet guestWallet,
        long amountVnd,
        DateTimeOffset now,
        string hostUserId,
        Guid playSessionPostId,
        Guid joinRequestId)
    {
        var balanceBefore = guestWallet.AvailableBalanceVnd;
        guestWallet.ReleaseHeld(amountVnd, now);
        guestWallet.CreditAvailable(amountVnd, now);

        dbContext.WalletTransactions.Add(WalletTransaction.CreateCompleted(
            guestWallet.UserId,
            WalletTransactionType.FullRefund,
            amountVnd,
            balanceBefore,
            guestWallet.AvailableBalanceVnd,
            "Host cancelled play session full refund",
            now,
            relatedUserId: hostUserId,
            playSessionPostId: playSessionPostId,
            joinRequestId: joinRequestId));
    }
}
