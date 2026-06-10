namespace BadmintonCourtBooking.Dtos;

public sealed record UserResponse(
    string Id,
    string Email,
    string FullName);
