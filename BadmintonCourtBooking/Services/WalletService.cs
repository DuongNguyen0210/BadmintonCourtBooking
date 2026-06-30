using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.Wallet;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class WalletService(
    ApplicationDbContext dbContext,
    IClock clock,
    IWalletAccountingService walletAccountingService) : IWalletService
{
    public async Task<WalletResponse> GetWalletAsync(string userId, CancellationToken cancellationToken)
    {
        var wallet = await walletAccountingService.GetOrCreateWalletAsync(userId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(wallet);
    }

    public async Task<IReadOnlyList<WalletTransactionResponse>> GetTransactionsAsync(string userId, CancellationToken cancellationToken)
    {
        return await dbContext.WalletTransactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId)
            .OrderByDescending(transaction => transaction.CreatedAtUtc)
            .Select(transaction => new WalletTransactionResponse(
                transaction.Id,
                transaction.Type.ToString(),
                transaction.Status.ToString(),
                transaction.AmountVnd,
                transaction.BalanceBeforeVnd,
                transaction.BalanceAfterVnd,
                transaction.Description,
                transaction.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<WalletResponse>> TopUpDevelopmentAsync(string userId, long amountVnd, CancellationToken cancellationToken)
    {
        if (amountVnd <= 0)
            return ServiceResult<WalletResponse>.Failure("INVALID_TOP_UP_AMOUNT", "Top-up amount must be greater than zero.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = clock.UtcNow;
        var wallet = await walletAccountingService.GetOrCreateWalletAsync(userId, cancellationToken);
        walletAccountingService.TopUp(
            wallet,
            amountVnd,
            now,
            $"development-top-up:{userId}:{Guid.NewGuid()}");

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<WalletResponse>.Success(ToResponse(wallet));
    }

    private static WalletResponse ToResponse(Wallet wallet)
    {
        return new WalletResponse(wallet.AvailableBalanceVnd, wallet.HeldBalanceVnd);
    }
}
