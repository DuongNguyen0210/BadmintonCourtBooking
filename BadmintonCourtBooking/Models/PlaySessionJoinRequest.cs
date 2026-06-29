namespace BadmintonCourtBooking.Models;

public sealed class PlaySessionJoinRequest
{
    private PlaySessionJoinRequest()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid PlaySessionPostId { get; private set; }

    public PlaySessionPost PlaySessionPost { get; private set; } = null!;

    public string GuestUserId { get; private set; } = string.Empty;

    public ApplicationUser GuestUser { get; private set; } = null!;

    public JoinRequestStatus Status { get; private set; } = JoinRequestStatus.PendingHostApproval;

    public DateTimeOffset RequestedAtUtc { get; private set; }

    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    public string? ReviewedByHostId { get; private set; }

    public DateTimeOffset? PaymentDueAtUtc { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static PlaySessionJoinRequest Create(Guid playSessionPostId, string guestUserId, DateTimeOffset now)
    {
        if (playSessionPostId == Guid.Empty)
            throw new ArgumentException("Play session post id is required.", nameof(playSessionPostId));

        if (string.IsNullOrWhiteSpace(guestUserId))
            throw new ArgumentException("Guest user id is required.", nameof(guestUserId));

        return new PlaySessionJoinRequest
        {
            PlaySessionPostId = playSessionPostId,
            GuestUserId = guestUserId,
            Status = JoinRequestStatus.PendingHostApproval,
            RequestedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Approve(string hostUserId, DateTimeOffset now, DateTimeOffset paymentDueAtUtc)
    {
        if (Status != JoinRequestStatus.PendingHostApproval)
            throw new DomainException("JOIN_REQUEST_NOT_PENDING", "Only pending join requests can be approved.");

        if (string.IsNullOrWhiteSpace(hostUserId))
            throw new ArgumentException("Host user id is required.", nameof(hostUserId));

        if (paymentDueAtUtc <= now)
            throw new ArgumentException("Payment due time must be after review time.", nameof(paymentDueAtUtc));

        Status = JoinRequestStatus.AwaitingPayment;
        ReviewedAtUtc = now;
        ReviewedByHostId = hostUserId;
        PaymentDueAtUtc = paymentDueAtUtc;
        UpdatedAtUtc = now;
    }

    public void Reject(string hostUserId, DateTimeOffset now)
    {
        if (Status != JoinRequestStatus.PendingHostApproval)
            throw new DomainException("JOIN_REQUEST_NOT_PENDING", "Only pending join requests can be rejected.");

        if (string.IsNullOrWhiteSpace(hostUserId))
            throw new ArgumentException("Host user id is required.", nameof(hostUserId));

        Status = JoinRequestStatus.Rejected;
        ReviewedAtUtc = now;
        ReviewedByHostId = hostUserId;
        UpdatedAtUtc = now;
    }

    public void MarkAsPaid(DateTimeOffset now)
    {
        if (Status != JoinRequestStatus.AwaitingPayment)
            throw new DomainException("JOIN_REQUEST_NOT_AWAITING_PAYMENT", "Only approved join requests can be paid.");

        if (PaymentDueAtUtc is null)
            throw new DomainException("PAYMENT_DUE_AT_MISSING", "Payment due time is missing.");

        if (PaymentDueAtUtc < now)
            throw new DomainException("JOIN_REQUEST_PAYMENT_EXPIRED", "Payment deadline has passed.");

        Status = JoinRequestStatus.Joined;
        PaidAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void Expire(DateTimeOffset now)
    {
        if (Status != JoinRequestStatus.AwaitingPayment)
            throw new DomainException("JOIN_REQUEST_NOT_AWAITING_PAYMENT", "Only awaiting payment requests can expire.");

        if (PaymentDueAtUtc is null)
            throw new DomainException("PAYMENT_DUE_AT_MISSING", "Payment due time is missing.");

        if (PaymentDueAtUtc > now)
            throw new DomainException("JOIN_REQUEST_PAYMENT_NOT_EXPIRED", "Payment deadline has not passed.");

        Status = JoinRequestStatus.Expired;
        UpdatedAtUtc = now;
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status is JoinRequestStatus.Rejected or JoinRequestStatus.Cancelled or JoinRequestStatus.Expired)
            throw new DomainException("JOIN_REQUEST_ALREADY_CLOSED", "Closed join requests cannot be cancelled.");

        Status = JoinRequestStatus.Cancelled;
        CancelledAtUtc = now;
        UpdatedAtUtc = now;
    }
}
