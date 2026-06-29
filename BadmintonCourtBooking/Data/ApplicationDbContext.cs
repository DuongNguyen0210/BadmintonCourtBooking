using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<PlaySessionPost> PlaySessionPosts => Set<PlaySessionPost>();
    public DbSet<PlaySessionJoinRequest> PlaySessionJoinRequests => Set<PlaySessionJoinRequest>();
    public DbSet<PlaySessionParticipant> PlaySessionParticipants => Set<PlaySessionParticipant>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<ParticipationCancellation> ParticipationCancellations => Set<ParticipationCancellation>();
    public DbSet<Notification> Notifications => Set<Notification>();

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
        });

        builder.Entity<PlaySessionJoinRequest>(entity =>
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
        });

        builder.Entity<PlaySessionParticipant>(entity =>
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
        });

        builder.Entity<Wallet>(entity =>
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
        });

        builder.Entity<WalletTransaction>(entity =>
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
        });

        builder.Entity<ParticipationCancellation>(entity =>
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
        });

        builder.Entity<Notification>(entity =>
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
        });
    }
}
