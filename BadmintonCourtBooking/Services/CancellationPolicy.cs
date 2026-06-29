using BadmintonCourtBooking.Options;

namespace BadmintonCourtBooking.Services;

public sealed class CancellationPolicy(CancellationPolicyOptions options) : ICancellationPolicy
{
    public CancellationQuote Quote(long originalAmountVnd)
    {
        if (originalAmountVnd <= 0)
            throw new ArgumentOutOfRangeException(nameof(originalAmountVnd), "Original amount must be greater than zero.");

        var refundRatePercent = GetRefundRatePercent(options.RefundRate);
        var refundAmountVnd = originalAmountVnd * refundRatePercent / 100;
        var cancellationFeeVnd = originalAmountVnd - refundAmountVnd;

        return new CancellationQuote(originalAmountVnd, refundAmountVnd, cancellationFeeVnd);
    }

    private static int GetRefundRatePercent(decimal refundRate)
    {
        if (refundRate < 0m || refundRate > 1m)
            throw new ArgumentOutOfRangeException(nameof(refundRate), "Refund rate must be between 0 and 1.");

        var percent = refundRate * 100m;

        if (percent != decimal.Truncate(percent))
            throw new ArgumentException("Refund rate must map to a whole percent.", nameof(refundRate));

        return (int)percent;
    }
}
