namespace BadmintonCourtBooking.Dtos.JoinRequests;

public sealed record JoinRequestResponse(
    Guid Id,
    Guid PlaySessionPostId,
    string PlaySessionTitle,
    string CourtName,
    string GuestUserId,
    string GuestName,
    string Status,
    long PricePerPlayerVnd,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset? PaymentDueAtUtc,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? CancelledAtUtc);
