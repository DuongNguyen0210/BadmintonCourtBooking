namespace BadmintonCourtBooking.Options;

public sealed class CancellationPolicyOptions
{
    public decimal RefundRate { get; init; } = 0.90m;
}
