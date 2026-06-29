namespace BadmintonCourtBooking.Dtos.Wallet;

public sealed record WalletResponse(
    long AvailableBalanceVnd,
    long HeldBalanceVnd);
