using BadmintonCourtBooking.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BadmintonCourtBooking.Tests.Integration;

public sealed class PostgresTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("badminton_court_booking_tests")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    public Task InitializeAsync()
    {
        return container.StartAsync();
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task DisposeAsync()
    {
        return container.DisposeAsync().AsTask();
    }
}
