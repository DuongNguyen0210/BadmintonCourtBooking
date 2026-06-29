using BadmintonCourtBooking.Dtos.Participations;

namespace BadmintonCourtBooking.Services;

public interface ICancellationService
{
    Task<ServiceResult<CancellationResponse>> CancelParticipationAsync(
        Guid participantId,
        string userId,
        CancelParticipationRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<object>> CancelPlaySessionByHostAsync(
        Guid playSessionPostId,
        string hostUserId,
        CancellationToken cancellationToken);
}
