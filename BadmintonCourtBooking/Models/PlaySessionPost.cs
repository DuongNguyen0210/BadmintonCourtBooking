namespace BadmintonCourtBooking.Models;

public sealed class PlaySessionPost
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string CreatorUserId { get; set; } = string.Empty;

    public ApplicationUser CreatorUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CourtName { get; set; } = string.Empty;

    public string CourtAddress { get; set; } = string.Empty;

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    // TODO: Remove legacy decimal price after frontend and older API consumers fully use PricePerPlayerVnd.
    public decimal PricePerPlayer { get; private set; }

    public long PricePerPlayerVnd { get; private set; }

    public int MaxPlayers { get; set; }

    // TODO: CurrentPlayers, MalePlayers, and FemalePlayers are manually entered in this phase.
    // Later, calculate these values from play session registration records.
    public int CurrentPlayers { get; set; }

    public int MalePlayers { get; set; }

    public int FemalePlayers { get; set; }

    public bool ShowMalePlayers { get; set; }

    public bool ShowFemalePlayers { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Active;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public void SetPricePerPlayer(decimal pricePerPlayer)
    {
        if (pricePerPlayer < 0)
            throw new ArgumentOutOfRangeException(nameof(pricePerPlayer), "Price per player must not be negative.");

        if (decimal.Truncate(pricePerPlayer) != pricePerPlayer)
            throw new ArgumentException("VND price must be a whole number.", nameof(pricePerPlayer));

        PricePerPlayer = pricePerPlayer;
        PricePerPlayerVnd = decimal.ToInt64(pricePerPlayer);
    }
}
