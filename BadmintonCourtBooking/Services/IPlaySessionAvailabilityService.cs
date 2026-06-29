using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IPlaySessionAvailabilityService
{
    Task<int> GetOccupiedSlotsAsync(
        PlaySessionPost post,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<bool> IsVisibleOnFeedAsync(
        PlaySessionPost post,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
