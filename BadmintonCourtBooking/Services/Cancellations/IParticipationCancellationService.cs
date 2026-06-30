using BadmintonCourtBooking.Dtos.Participations;

namespace BadmintonCourtBooking.Services;

public interface IParticipationCancellationService
{
    Task<ServiceResult<CancellationResponse>> CancelParticipationAsync(
        Guid participantId,
        string userId,
        CancelParticipationRequest request,
        CancellationToken cancellationToken);
}
