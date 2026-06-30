using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonCourtBooking.Data.Configurations;

public sealed class PlaySessionPostConfiguration : IEntityTypeConfiguration<PlaySessionPost>
{
    public void Configure(EntityTypeBuilder<PlaySessionPost> entity)
    {
        entity.HasKey(post => post.Id);

        entity.Property(post => post.Title)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(post => post.Description)
            .HasMaxLength(1000)
            .IsRequired();

        entity.Property(post => post.CourtName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(post => post.CourtAddress)
            .HasMaxLength(300)
            .IsRequired();

        entity.Property(post => post.PricePerPlayer)
            .HasPrecision(18, 2);

        entity.Property(post => post.PricePerPlayerVnd)
            .IsRequired();

        entity.Property(post => post.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.HasOne(post => post.CreatorUser)
            .WithMany()
            .HasForeignKey(post => post.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasIndex(post => post.CreatorUserId);
        entity.HasIndex(post => post.Status);
        entity.HasIndex(post => post.StartTime);
        entity.HasIndex(post => post.EndTime);
    }
}
