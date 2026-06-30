using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonCourtBooking.Data.Configurations;

public sealed class PlaySessionParticipantConfiguration : IEntityTypeConfiguration<PlaySessionParticipant>
{
    public void Configure(EntityTypeBuilder<PlaySessionParticipant> entity)
    {
        entity.HasKey(participant => participant.Id);

        entity.Property(participant => participant.UserId)
            .IsRequired();

        entity.Property(participant => participant.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.HasOne(participant => participant.PlaySessionPost)
            .WithMany()
            .HasForeignKey(participant => participant.PlaySessionPostId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasOne(participant => participant.User)
            .WithMany()
            .HasForeignKey(participant => participant.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasOne(participant => participant.JoinRequest)
            .WithMany()
            .HasForeignKey(participant => participant.JoinRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasIndex(participant => new { participant.PlaySessionPostId, participant.Status });
        entity.HasIndex(participant => new { participant.UserId, participant.Status });
        entity.HasIndex(participant => participant.JoinRequestId)
            .IsUnique();
        entity.HasIndex(participant => new { participant.PlaySessionPostId, participant.UserId })
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");
    }
}
