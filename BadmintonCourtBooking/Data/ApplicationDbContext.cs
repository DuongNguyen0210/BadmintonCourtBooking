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
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
