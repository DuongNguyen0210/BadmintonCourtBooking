namespace BadmintonCourtBooking.Models;

public sealed class Notification
{
    private Notification()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string RecipientUserId { get; private set; } = string.Empty;

    public ApplicationUser RecipientUser { get; private set; } = null!;

    public NotificationType Type { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string? RelatedEntityId { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ReadAtUtc { get; private set; }

    public static Notification Create(
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        DateTimeOffset now,
        string? relatedEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId))
            throw new ArgumentException("Recipient user id is required.", nameof(recipientUserId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));

        return new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            RelatedEntityId = string.IsNullOrWhiteSpace(relatedEntityId) ? null : relatedEntityId.Trim(),
            CreatedAtUtc = now
        };
    }

    public void MarkAsRead(DateTimeOffset now)
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAtUtc = now;
    }
}
