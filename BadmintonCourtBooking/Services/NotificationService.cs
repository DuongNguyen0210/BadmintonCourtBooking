using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.Notifications;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class NotificationService(ApplicationDbContext dbContext, IClock clock) : INotificationService
{
    public void Add(
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        DateTimeOffset now,
        string? relatedEntityId)
    {
        dbContext.Notifications.Add(Notification.Create(
            recipientUserId,
            type,
            title,
            message,
            now,
            relatedEntityId));
    }

    public async Task CreateAsync(
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityId,
        CancellationToken cancellationToken)
    {
        Add(
            recipientUserId,
            type,
            title,
            message,
            clock.UtcNow,
            relatedEntityId);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetMineAsync(string userId, CancellationToken cancellationToken)
    {
        return await dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == userId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Select(notification => ToResponse(notification))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<NotificationResponse>> MarkAsReadAsync(string userId, Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(
            existingNotification => existingNotification.Id == notificationId,
            cancellationToken);

        if (notification is null)
            return ServiceResult<NotificationResponse>.Failure("NOTIFICATION_NOT_FOUND", "Notification was not found.");

        if (notification.RecipientUserId != userId)
            return ServiceResult<NotificationResponse>.Failure("UNAUTHORIZED_NOTIFICATION_ACCESS", "You cannot access this notification.");

        notification.MarkAsRead(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<NotificationResponse>.Success(ToResponse(notification));
    }

    private static NotificationResponse ToResponse(Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.RelatedEntityId,
            notification.IsRead,
            notification.CreatedAtUtc,
            notification.ReadAtUtc);
    }
}
