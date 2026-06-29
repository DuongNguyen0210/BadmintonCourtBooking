using BadmintonCourtBooking.Options;
using BadmintonCourtBooking.Services;

namespace BadmintonCourtBooking.Tests.Unit;

public sealed class CancellationPolicyTests
{
    [Theory]
    [InlineData(100000, 90000, 10000)]
    [InlineData(100001, 90000, 10001)]
    [InlineData(1, 0, 1)]
    public void Quote_UsesIntegerMathAndKeepsRemainderAsFee(
        long originalAmountVnd,
        long expectedRefundAmountVnd,
        long expectedCancellationFeeVnd)
    {
        var policy = new CancellationPolicy(new CancellationPolicyOptions());

        var quote = policy.Quote(originalAmountVnd);

        Assert.Equal(originalAmountVnd, quote.OriginalAmountVnd);
        Assert.Equal(expectedRefundAmountVnd, quote.RefundAmountVnd);
        Assert.Equal(expectedCancellationFeeVnd, quote.CancellationFeeVnd);
        Assert.Equal(originalAmountVnd, quote.RefundAmountVnd + quote.CancellationFeeVnd);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quote_RejectsNonPositiveOriginalAmount(long originalAmountVnd)
    {
        var policy = new CancellationPolicy(new CancellationPolicyOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => policy.Quote(originalAmountVnd));
    }
}
