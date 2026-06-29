namespace BadmintonCourtBooking.Models;

public sealed class PlaySessionParticipant
{
    private PlaySessionParticipant()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid PlaySessionPostId { get; private set; }

    public PlaySessionPost PlaySessionPost { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;

    public ApplicationUser User { get; private set; } = null!;

    public Guid JoinRequestId { get; private set; }

    public PlaySessionJoinRequest JoinRequest { get; private set; } = null!;

    public ParticipantStatus Status { get; private set; } = ParticipantStatus.Active;

    public DateTimeOffset JoinedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public static PlaySessionParticipant Create(Guid playSessionPostId, string userId, Guid joinRequestId, DateTimeOffset now)
    {
        if (playSessionPostId == Guid.Empty)
            throw new ArgumentException("Play session post id is required.", nameof(playSessionPostId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        if (joinRequestId == Guid.Empty)
            throw new ArgumentException("Join request id is required.", nameof(joinRequestId));

        return new PlaySessionParticipant
        {
            PlaySessionPostId = playSessionPostId,
            UserId = userId,
            JoinRequestId = joinRequestId,
            Status = ParticipantStatus.Active,
            JoinedAtUtc = now
        };
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status != ParticipantStatus.Active)
            throw new DomainException("PARTICIPANT_ALREADY_CANCELLED", "Only active participants can be cancelled.");

        Status = ParticipantStatus.Cancelled;
        CancelledAtUtc = now;
    }
}
