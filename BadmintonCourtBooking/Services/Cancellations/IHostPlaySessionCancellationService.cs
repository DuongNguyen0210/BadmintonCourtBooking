namespace BadmintonCourtBooking.Services;

public interface IHostPlaySessionCancellationService
{
    Task<ServiceResult<object>> CancelPlaySessionByHostAsync(
        Guid playSessionPostId,
        string hostUserId,
        CancellationToken cancellationToken);
}
