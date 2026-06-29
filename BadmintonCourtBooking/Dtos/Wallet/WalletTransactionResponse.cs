namespace BadmintonCourtBooking.Dtos.Wallet;

public sealed record WalletTransactionResponse(
    Guid Id,
    string Type,
    string Status,
    long AmountVnd,
    long BalanceBeforeVnd,
    long BalanceAfterVnd,
    string Description,
    DateTimeOffset CreatedAtUtc);
