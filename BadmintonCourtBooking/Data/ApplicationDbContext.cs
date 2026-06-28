using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<PlaySessionPost> PlaySessionPosts => Set<PlaySessionPost>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Entity<PlaySessionPost>(entity =>
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
        });
    }
}
