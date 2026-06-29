using BadmintonCourtBooking.Dtos.Wallet;

namespace BadmintonCourtBooking.Services;

public interface IWalletService
{
    Task<WalletResponse> GetWalletAsync(string userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<WalletTransactionResponse>> GetTransactionsAsync(string userId, CancellationToken cancellationToken);

    Task<ServiceResult<WalletResponse>> TopUpDevelopmentAsync(string userId, long amountVnd, CancellationToken cancellationToken);
}
