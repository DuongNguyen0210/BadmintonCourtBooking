using BadmintonCourtBooking.Dtos.JoinRequests;
using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IJoinRequestService
{
    Task<ServiceResult<JoinRequestResponse>> RequestToJoinAsync(
        Guid playSessionPostId,
        string guestUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<JoinRequestResponse>> GetMineAsync(
        string guestUserId,
        CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<JoinRequestResponse>>> GetForHostAsync(
        string hostUserId,
        JoinRequestStatus? status,
        CancellationToken cancellationToken);

    Task<ServiceResult<JoinRequestResponse>> ApproveAsync(
        Guid joinRequestId,
        string hostUserId,
        CancellationToken cancellationToken);

    Task<ServiceResult<JoinRequestResponse>> RejectAsync(
        Guid joinRequestId,
        string hostUserId,
        CancellationToken cancellationToken);
}
