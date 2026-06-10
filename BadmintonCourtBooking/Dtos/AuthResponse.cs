namespace BadmintonCourtBooking.Dtos;

public sealed record AuthResponse(
    bool Success,
    string Message,
    UserResponse? User);
