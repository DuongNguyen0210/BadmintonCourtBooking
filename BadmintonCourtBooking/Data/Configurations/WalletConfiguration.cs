using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonCourtBooking.Data.Configurations;

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> entity)
    {
        entity.HasKey(wallet => wallet.Id);

        entity.Property(wallet => wallet.UserId)
            .IsRequired();

        entity.Property(wallet => wallet.ConcurrencyToken)
            .IsConcurrencyToken()
            .IsRequired();

        entity.HasOne(wallet => wallet.User)
            .WithMany()
            .HasForeignKey(wallet => wallet.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasIndex(wallet => wallet.UserId)
            .IsUnique();
    }
}
