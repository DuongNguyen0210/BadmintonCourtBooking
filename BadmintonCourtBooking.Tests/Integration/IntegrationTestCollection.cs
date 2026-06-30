namespace BadmintonCourtBooking.Tests.Integration;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgresTestContainerFixture>
{
    public const string Name = "Integration";
}
