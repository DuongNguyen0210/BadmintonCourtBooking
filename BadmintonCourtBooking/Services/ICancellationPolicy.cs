namespace BadmintonCourtBooking.Services;

public interface ICancellationPolicy
{
    CancellationQuote Quote(long originalAmountVnd);
}
