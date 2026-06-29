namespace BadmintonCourtBooking.Services;

public sealed record CancellationQuote(
    long OriginalAmountVnd,
    long RefundAmountVnd,
    long CancellationFeeVnd);
