using BadmintonCourtBooking.Dtos.Notifications;
using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface INotificationService
{
    void Add(
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        DateTimeOffset now,
        string? relatedEntityId);

    Task CreateAsync(
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationResponse>> GetMineAsync(string userId, CancellationToken cancellationToken);

    Task<ServiceResult<NotificationResponse>> MarkAsReadAsync(string userId, Guid notificationId, CancellationToken cancellationToken);
}
