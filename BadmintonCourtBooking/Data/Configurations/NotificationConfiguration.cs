using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonCourtBooking.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.HasKey(notification => notification.Id);

        entity.Property(notification => notification.RecipientUserId)
            .IsRequired();

        entity.Property(notification => notification.Type)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(notification => notification.Title)
            .HasMaxLength(160)
            .IsRequired();

        entity.Property(notification => notification.Message)
            .HasMaxLength(1000)
            .IsRequired();

        entity.Property(notification => notification.RelatedEntityId)
            .HasMaxLength(100);

        entity.HasOne(notification => notification.RecipientUser)
            .WithMany()
            .HasForeignKey(notification => notification.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasIndex(notification => new { notification.RecipientUserId, notification.IsRead });
        entity.HasIndex(notification => notification.CreatedAtUtc);
    }
}
