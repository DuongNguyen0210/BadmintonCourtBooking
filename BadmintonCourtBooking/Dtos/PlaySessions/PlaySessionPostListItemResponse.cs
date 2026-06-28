namespace BadmintonCourtBooking.Dtos.PlaySessions;

public sealed record PlaySessionPostListItemResponse(
    Guid Id,
    string Title,
    string CourtName,
    string CourtAddress,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal PricePerPlayer,
    int MaxPlayers,
    int CurrentPlayers,
    int? MalePlayers,
    int? FemalePlayers,
    string Status,
    string CreatorName,
    bool CanManage);
