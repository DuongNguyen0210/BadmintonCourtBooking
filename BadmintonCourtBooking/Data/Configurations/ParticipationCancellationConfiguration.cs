using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonCourtBooking.Data.Configurations;

public sealed class ParticipationCancellationConfiguration : IEntityTypeConfiguration<ParticipationCancellation>
{
    public void Configure(EntityTypeBuilder<ParticipationCancellation> entity)
    {
        entity.HasKey(cancellation => cancellation.Id);

        entity.Property(cancellation => cancellation.RequestedByUserId)
            .IsRequired();

        entity.Property(cancellation => cancellation.RefundChoice)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(cancellation => cancellation.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(cancellation => cancellation.Reason)
            .HasMaxLength(500);

        entity.HasOne(cancellation => cancellation.Participant)
            .WithMany()
            .HasForeignKey(cancellation => cancellation.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(cancellation => cancellation.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasIndex(cancellation => cancellation.ParticipantId)
            .IsUnique();
        entity.HasIndex(cancellation => cancellation.RequestedByUserId);
    }
}
