using BadmintonCourtBooking.Dtos.PlaySessions;
using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Features.PlaySessions;

public static class PlaySessionPostMapper
{
    public static PlaySessionPostListItemResponse ToListItemResponse(
        PlaySessionPost post,
        string currentUserId,
        int occupiedSlots)
    {
        return new PlaySessionPostListItemResponse(
            post.Id,
            post.Title,
            post.CourtName,
            post.CourtAddress,
            post.StartTime,
            post.EndTime,
            post.PricePerPlayer,
            post.PricePerPlayerVnd,
            post.MaxPlayers,
            occupiedSlots,
            post.ShowMalePlayers ? post.MalePlayers : null,
            post.ShowFemalePlayers ? post.FemalePlayers : null,
            post.Status.ToString(),
            post.CreatorUser.FullName,
            post.CreatorUserId == currentUserId);
    }

    public static PlaySessionPostDetailResponse ToDetailResponse(
        PlaySessionPost post,
        string currentUserId,
        int occupiedSlots)
    {
        return new PlaySessionPostDetailResponse(
            post.Id,
            post.Title,
            post.Description,
            post.CourtName,
            post.CourtAddress,
            post.StartTime,
            post.EndTime,
            post.PricePerPlayer,
            post.PricePerPlayerVnd,
            post.MaxPlayers,
            occupiedSlots,
            post.ShowMalePlayers ? post.MalePlayers : null,
            post.ShowFemalePlayers ? post.FemalePlayers : null,
            post.ShowMalePlayers,
            post.ShowFemalePlayers,
            post.Status.ToString(),
            post.CreatorUserId,
            post.CreatorUser.FullName,
            post.CreatedAt,
            post.UpdatedAt,
            post.CreatorUserId == currentUserId);
    }
}
