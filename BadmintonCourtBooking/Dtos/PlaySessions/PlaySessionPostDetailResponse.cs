namespace BadmintonCourtBooking.Dtos.PlaySessions;

public sealed record PlaySessionPostDetailResponse(
    Guid Id,
    string Title,
    string Description,
    string CourtName,
    string CourtAddress,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal PricePerPlayer,
    int MaxPlayers,
    int CurrentPlayers,
    int? MalePlayers,
    int? FemalePlayers,
    bool ShowMalePlayers,
    bool ShowFemalePlayers,
    string Status,
    string CreatorUserId,
    string CreatorName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool CanManage);
