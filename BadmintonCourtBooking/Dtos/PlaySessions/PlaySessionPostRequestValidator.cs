using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Dtos.PlaySessions;

internal static class PlaySessionPostRequestValidator
{
    public static IEnumerable<ValidationResult> Validate(
        string title,
        string courtName,
        string courtAddress,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal pricePerPlayer,
        int maxPlayers,
        int currentPlayers,
        int malePlayers,
        int femalePlayers)
    {
        if (string.IsNullOrWhiteSpace(title))
            yield return new ValidationResult("Title is required.", [nameof(title)]);

        if (string.IsNullOrWhiteSpace(courtName))
            yield return new ValidationResult("Court name is required.", [nameof(courtName)]);

        if (string.IsNullOrWhiteSpace(courtAddress))
            yield return new ValidationResult("Court address is required.", [nameof(courtAddress)]);

        if (startTime >= endTime)
            yield return new ValidationResult("Start time must be earlier than end time.", [nameof(startTime), nameof(endTime)]);

        if (pricePerPlayer < 0)
            yield return new ValidationResult("Price per player must be greater than or equal to 0.", [nameof(pricePerPlayer)]);

        if (decimal.Truncate(pricePerPlayer) != pricePerPlayer)
            yield return new ValidationResult("Price per player VND must be a whole number.", [nameof(pricePerPlayer)]);

        if (maxPlayers <= 0)
            yield return new ValidationResult("Max players must be greater than 0.", [nameof(maxPlayers)]);

        if (currentPlayers < 0)
            yield return new ValidationResult("Current players must be greater than or equal to 0.", [nameof(currentPlayers)]);

        if (currentPlayers > maxPlayers)
            yield return new ValidationResult("Current players must not exceed max players.", [nameof(currentPlayers), nameof(maxPlayers)]);

        if (malePlayers < 0)
            yield return new ValidationResult("Male players must be greater than or equal to 0.", [nameof(malePlayers)]);

        if (femalePlayers < 0)
            yield return new ValidationResult("Female players must be greater than or equal to 0.", [nameof(femalePlayers)]);

        if (malePlayers + femalePlayers > currentPlayers)
            yield return new ValidationResult("Male and female players must not exceed current players.", [nameof(malePlayers), nameof(femalePlayers), nameof(currentPlayers)]);
    }
}
