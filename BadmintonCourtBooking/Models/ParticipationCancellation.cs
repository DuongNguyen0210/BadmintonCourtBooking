namespace BadmintonCourtBooking.Models;

public sealed class ParticipationCancellation
{
    private ParticipationCancellation()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid ParticipantId { get; private set; }

    public PlaySessionParticipant Participant { get; private set; } = null!;

    public string RequestedByUserId { get; private set; } = string.Empty;

    public CancellationRefundChoice RefundChoice { get; private set; }

    public long OriginalAmountVnd { get; private set; }

    public long RefundAmountVnd { get; private set; }

    public long CancellationFeeVnd { get; private set; }

    public CancellationStatus Status { get; private set; } = CancellationStatus.Completed;

    public string? Reason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static ParticipationCancellation Complete(
        Guid participantId,
        string requestedByUserId,
        CancellationRefundChoice refundChoice,
        long originalAmountVnd,
        long refundAmountVnd,
        long cancellationFeeVnd,
        DateTimeOffset now,
        string? reason)
    {
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id is required.", nameof(participantId));

        if (string.IsNullOrWhiteSpace(requestedByUserId))
            throw new ArgumentException("Requested by user id is required.", nameof(requestedByUserId));

        if (originalAmountVnd <= 0)
            throw new ArgumentOutOfRangeException(nameof(originalAmountVnd), "Original amount must be greater than zero.");

        if (refundAmountVnd < 0)
            throw new ArgumentOutOfRangeException(nameof(refundAmountVnd), "Refund amount cannot be negative.");

        if (cancellationFeeVnd < 0)
            throw new ArgumentOutOfRangeException(nameof(cancellationFeeVnd), "Cancellation fee cannot be negative.");

        if (refundAmountVnd + cancellationFeeVnd != originalAmountVnd)
            throw new ArgumentException("Refund amount and cancellation fee must equal original amount.");

        return new ParticipationCancellation
        {
            ParticipantId = participantId,
            RequestedByUserId = requestedByUserId,
            RefundChoice = refundChoice,
            OriginalAmountVnd = originalAmountVnd,
            RefundAmountVnd = refundAmountVnd,
            CancellationFeeVnd = cancellationFeeVnd,
            Status = CancellationStatus.Completed,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedAtUtc = now,
            CompletedAtUtc = now
        };
    }
}
