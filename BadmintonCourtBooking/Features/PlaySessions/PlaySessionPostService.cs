using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.PlaySessions;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Features.PlaySessions;

public sealed class PlaySessionPostService(
    ApplicationDbContext dbContext,
    ICancellationService cancellationService,
    IPlaySessionAvailabilityService availabilityService,
    IClock clock) : IPlaySessionPostService
{
    public async Task<IReadOnlyList<PlaySessionPostListItemResponse>> GetFeedAsync(
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        var candidatePosts = await dbContext.PlaySessionPosts
            .Include(post => post.CreatorUser)
            .Where(post =>
                post.Status == PostStatus.Active &&
                post.StartTime > now)
            .OrderBy(post => post.StartTime)
            .ThenByDescending(post => post.CreatedAt)
            .ToListAsync(cancellationToken);

        var visiblePosts = new List<PlaySessionPostListItemResponse>();
        foreach (var post in candidatePosts)
        {
            if (!await availabilityService.IsVisibleOnFeedAsync(post, now, cancellationToken))
                continue;

            var occupiedSlots = await availabilityService.GetOccupiedSlotsAsync(post, now, cancellationToken);
            visiblePosts.Add(PlaySessionPostMapper.ToListItemResponse(post, currentUserId, occupiedSlots));
        }

        return visiblePosts;
    }

    public async Task<ServiceResult<PlaySessionPostDetailResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var post = await dbContext.PlaySessionPosts
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstOrDefaultAsync(playSessionPost => playSessionPost.Id == id, cancellationToken);

        if (post is null)
            return ServiceResult<PlaySessionPostDetailResponse>.Failure("PLAY_SESSION_NOT_FOUND", "Play session was not found.");

        var occupiedSlots = await availabilityService.GetOccupiedSlotsAsync(post, clock.UtcNow, cancellationToken);

        return ServiceResult<PlaySessionPostDetailResponse>.Success(
            PlaySessionPostMapper.ToDetailResponse(post, currentUserId, occupiedSlots));
    }

    public async Task<ServiceResult<PlaySessionPostDetailResponse>> CreateAsync(
        CreatePlaySessionPostRequest request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var post = new PlaySessionPost
        {
            CreatorUserId = currentUserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CourtName = request.CourtName.Trim(),
            CourtAddress = request.CourtAddress.Trim(),
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            MaxPlayers = request.MaxPlayers,
            CurrentPlayers = request.CurrentPlayers,
            MalePlayers = request.MalePlayers,
            FemalePlayers = request.FemalePlayers,
            ShowMalePlayers = request.ShowMalePlayers,
            ShowFemalePlayers = request.ShowFemalePlayers,
            Status = PostStatus.Active,
            CreatedAt = now
        };
        post.SetPricePerPlayer(request.PricePerPlayer);

        dbContext.PlaySessionPosts.Add(post);
        await dbContext.SaveChangesAsync(cancellationToken);

        var createdPost = await dbContext.PlaySessionPosts
            .AsNoTracking()
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstAsync(playSessionPost => playSessionPost.Id == post.Id, cancellationToken);

        return ServiceResult<PlaySessionPostDetailResponse>.Success(
            PlaySessionPostMapper.ToDetailResponse(createdPost, currentUserId, createdPost.CurrentPlayers));
    }

    public async Task<ServiceResult<PlaySessionPostDetailResponse>> UpdateAsync(
        Guid id,
        UpdatePlaySessionPostRequest request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var post = await dbContext.PlaySessionPosts
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstOrDefaultAsync(playSessionPost => playSessionPost.Id == id, cancellationToken);

        if (post is null)
            return ServiceResult<PlaySessionPostDetailResponse>.Failure("PLAY_SESSION_NOT_FOUND", "Play session was not found.");

        if (post.CreatorUserId != currentUserId)
            return ServiceResult<PlaySessionPostDetailResponse>.Failure("FORBIDDEN", "Only the host can update this play session.");

        post.Title = request.Title.Trim();
        post.Description = request.Description.Trim();
        post.CourtName = request.CourtName.Trim();
        post.CourtAddress = request.CourtAddress.Trim();
        post.StartTime = request.StartTime.ToUniversalTime();
        post.EndTime = request.EndTime.ToUniversalTime();
        post.SetPricePerPlayer(request.PricePerPlayer);
        post.MaxPlayers = request.MaxPlayers;
        post.CurrentPlayers = request.CurrentPlayers;
        post.MalePlayers = request.MalePlayers;
        post.FemalePlayers = request.FemalePlayers;
        post.ShowMalePlayers = request.ShowMalePlayers;
        post.ShowFemalePlayers = request.ShowFemalePlayers;
        post.UpdatedAt = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<PlaySessionPostDetailResponse>.Success(
            PlaySessionPostMapper.ToDetailResponse(post, currentUserId, post.CurrentPlayers));
    }

    public Task<ServiceResult<object>> CancelAsync(
        Guid id,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        return cancellationService.CancelPlaySessionByHostAsync(id, currentUserId, cancellationToken);
    }
}
