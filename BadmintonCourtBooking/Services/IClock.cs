namespace BadmintonCourtBooking.Services;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
