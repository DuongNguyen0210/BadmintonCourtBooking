using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonCourtBooking.Data.Configurations;

public sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> entity)
    {
        entity.HasKey(transaction => transaction.Id);

        entity.Property(transaction => transaction.UserId)
            .IsRequired();

        entity.Property(transaction => transaction.RelatedUserId)
            .HasMaxLength(450);

        entity.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(transaction => transaction.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(transaction => transaction.Description)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(transaction => transaction.IdempotencyKey)
            .HasMaxLength(200);

        entity.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(transaction => transaction.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(transaction => transaction.RelatedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<PlaySessionPost>()
            .WithMany()
            .HasForeignKey(transaction => transaction.PlaySessionPostId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<PlaySessionJoinRequest>()
            .WithMany()
            .HasForeignKey(transaction => transaction.JoinRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<ParticipationCancellation>()
            .WithMany()
            .HasForeignKey(transaction => transaction.CancellationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(transaction => new { transaction.UserId, transaction.CreatedAtUtc });
        entity.HasIndex(transaction => transaction.PlaySessionPostId);
        entity.HasIndex(transaction => transaction.JoinRequestId);
        entity.HasIndex(transaction => transaction.CancellationId);
        entity.HasIndex(transaction => transaction.RelatedUserId);
        entity.HasIndex(transaction => transaction.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");
    }
}
