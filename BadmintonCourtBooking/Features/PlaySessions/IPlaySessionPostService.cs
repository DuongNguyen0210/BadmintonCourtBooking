using BadmintonCourtBooking.Dtos.PlaySessions;
using BadmintonCourtBooking.Services;

namespace BadmintonCourtBooking.Features.PlaySessions;

public interface IPlaySessionPostService
{
    Task<IReadOnlyList<PlaySessionPostListItemResponse>> GetFeedAsync(
        string currentUserId,
        CancellationToken cancellationToken);

    Task<ServiceResult<PlaySessionPostDetailResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        CancellationToken cancellationToken);

    Task<ServiceResult<PlaySessionPostDetailResponse>> CreateAsync(
        CreatePlaySessionPostRequest request,
        string currentUserId,
        CancellationToken cancellationToken);

    Task<ServiceResult<PlaySessionPostDetailResponse>> UpdateAsync(
        Guid id,
        UpdatePlaySessionPostRequest request,
        string currentUserId,
        CancellationToken cancellationToken);

    Task<ServiceResult<object>> CancelAsync(
        Guid id,
        string currentUserId,
        CancellationToken cancellationToken);
}
