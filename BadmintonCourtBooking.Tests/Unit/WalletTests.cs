using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Tests.Unit;

public sealed class WalletTests
{
    [Fact]
    public void MoveAvailableToHeld_DeductsAvailableAndIncreasesHeld()
    {
        var now = new DateTimeOffset(2026, 6, 29, 10, 0, 0, TimeSpan.Zero);
        var wallet = Wallet.Create("user-1", now);
        wallet.CreditAvailable(100000, now);

        wallet.MoveAvailableToHeld(60000, now.AddMinutes(1));

        Assert.Equal(40000, wallet.AvailableBalanceVnd);
        Assert.Equal(60000, wallet.HeldBalanceVnd);
    }

    [Fact]
    public void DebitAvailable_RejectsInsufficientBalance()
    {
        var now = new DateTimeOffset(2026, 6, 29, 10, 0, 0, TimeSpan.Zero);
        var wallet = Wallet.Create("user-1", now);

        var exception = Assert.Throws<DomainException>(() => wallet.DebitAvailable(1, now));

        Assert.Equal("INSUFFICIENT_BALANCE", exception.Code);
    }
}
