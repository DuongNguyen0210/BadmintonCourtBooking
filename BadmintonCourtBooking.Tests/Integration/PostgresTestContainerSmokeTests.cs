namespace BadmintonCourtBooking.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class PostgresTestContainerSmokeTests(PostgresTestContainerFixture fixture)
{
    [Fact]
    public async Task Test_container_provides_isolated_postgresql_database()
    {
        await fixture.ResetDatabaseAsync();

        await using var dbContext = fixture.CreateDbContext();

        Assert.True(await dbContext.Database.CanConnectAsync());
    }
}
