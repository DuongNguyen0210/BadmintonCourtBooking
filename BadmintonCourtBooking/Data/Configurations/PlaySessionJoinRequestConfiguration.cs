using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonCourtBooking.Data.Configurations;

public sealed class PlaySessionJoinRequestConfiguration : IEntityTypeConfiguration<PlaySessionJoinRequest>
{
    public void Configure(EntityTypeBuilder<PlaySessionJoinRequest> entity)
    {
        entity.HasKey(request => request.Id);

        entity.Property(request => request.GuestUserId)
            .IsRequired();

        entity.Property(request => request.ReviewedByHostId)
            .HasMaxLength(450);

        entity.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        entity.HasOne(request => request.PlaySessionPost)
            .WithMany()
            .HasForeignKey(request => request.PlaySessionPostId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasOne(request => request.GuestUser)
            .WithMany()
            .HasForeignKey(request => request.GuestUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasIndex(request => new { request.PlaySessionPostId, request.Status });
        entity.HasIndex(request => new { request.GuestUserId, request.Status });
        entity.HasIndex(request => request.PaymentDueAtUtc);
        entity.HasIndex(request => new { request.PlaySessionPostId, request.GuestUserId })
            .IsUnique()
            .HasFilter("\"Status\" IN ('PendingHostApproval', 'AwaitingPayment', 'Joined')");
    }
}
