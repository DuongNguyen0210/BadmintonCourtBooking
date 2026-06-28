using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Dtos.PlaySessions;

public sealed class UpdatePlaySessionPostRequest : IValidatableObject
{
    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CourtName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string CourtAddress { get; set; } = string.Empty;

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public decimal PricePerPlayer { get; set; }

    public int MaxPlayers { get; set; }

    public int CurrentPlayers { get; set; }

    public int MalePlayers { get; set; }

    public int FemalePlayers { get; set; }

    public bool ShowMalePlayers { get; set; }

    public bool ShowFemalePlayers { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return PlaySessionPostRequestValidator.Validate(
            Title,
            CourtName,
            CourtAddress,
            StartTime,
            EndTime,
            PricePerPlayer,
            MaxPlayers,
            CurrentPlayers,
            MalePlayers,
            FemalePlayers);
    }
}
