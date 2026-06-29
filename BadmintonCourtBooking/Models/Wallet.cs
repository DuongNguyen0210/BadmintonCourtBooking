namespace BadmintonCourtBooking.Models;

public sealed class Wallet
{
    private Wallet()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string UserId { get; private set; } = string.Empty;

    public ApplicationUser User { get; private set; } = null!;

    public long AvailableBalanceVnd { get; private set; }

    public long HeldBalanceVnd { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public byte[] ConcurrencyToken { get; private set; } = [];

    public static Wallet Create(string userId, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        return new Wallet
        {
            UserId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            ConcurrencyToken = Guid.NewGuid().ToByteArray()
        };
    }

    public void CreditAvailable(long amountVnd, DateTimeOffset now)
    {
        EnsurePositiveAmount(amountVnd);

        AvailableBalanceVnd += amountVnd;
        UpdatedAtUtc = now;
        RefreshConcurrencyToken();
    }

    public void DebitAvailable(long amountVnd, DateTimeOffset now)
    {
        EnsurePositiveAmount(amountVnd);

        if (AvailableBalanceVnd < amountVnd)
            throw new DomainException("INSUFFICIENT_BALANCE", "Available balance is not enough.");

        AvailableBalanceVnd -= amountVnd;
        UpdatedAtUtc = now;
        RefreshConcurrencyToken();
    }

    public void MoveAvailableToHeld(long amountVnd, DateTimeOffset now)
    {
        DebitAvailable(amountVnd, now);
        HeldBalanceVnd += amountVnd;
        UpdatedAtUtc = now;
        RefreshConcurrencyToken();
    }

    public void ReleaseHeld(long amountVnd, DateTimeOffset now)
    {
        EnsurePositiveAmount(amountVnd);

        if (HeldBalanceVnd < amountVnd)
            throw new DomainException("INSUFFICIENT_HELD_BALANCE", "Held balance is not enough.");

        HeldBalanceVnd -= amountVnd;
        UpdatedAtUtc = now;
        RefreshConcurrencyToken();
    }

    private static void EnsurePositiveAmount(long amountVnd)
    {
        if (amountVnd <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountVnd), "Amount must be greater than zero.");
    }

    private void RefreshConcurrencyToken()
    {
        ConcurrencyToken = Guid.NewGuid().ToByteArray();
    }
}
