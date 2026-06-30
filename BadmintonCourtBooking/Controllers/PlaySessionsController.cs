using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.PlaySessions;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/play-sessions")]
public sealed class PlaySessionsController(
    ApplicationDbContext dbContext,
    ICancellationService cancellationService,
    IPlaySessionAvailabilityService availabilityService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlaySessionPostListItemResponse>>> GetFeed(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserAccessor.GetRequiredUserId(User);
        var now = DateTimeOffset.UtcNow;

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
            visiblePosts.Add(ToListItemResponse(post, currentUserId, occupiedSlots));
        }

        return Ok(visiblePosts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaySessionPostDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserAccessor.GetRequiredUserId(User);
        var post = await dbContext.PlaySessionPosts
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstOrDefaultAsync(playSessionPost => playSessionPost.Id == id, cancellationToken);

        if (post is null)
            return NotFound();

        var occupiedSlots = await availabilityService.GetOccupiedSlotsAsync(post, DateTimeOffset.UtcNow, cancellationToken);

        return Ok(ToDetailResponse(post, currentUserId, occupiedSlots));
    }

    [HttpPost]
    public async Task<ActionResult<PlaySessionPostDetailResponse>> Create(CreatePlaySessionPostRequest request)
    {
        var currentUserId = currentUserAccessor.GetRequiredUserId(User);
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
            CreatedAt = DateTimeOffset.UtcNow
        };
        post.SetPricePerPlayer(request.PricePerPlayer);

        dbContext.PlaySessionPosts.Add(post);
        await dbContext.SaveChangesAsync();

        var createdPost = await dbContext.PlaySessionPosts
            .AsNoTracking()
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstAsync(playSessionPost => playSessionPost.Id == post.Id);

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, ToDetailResponse(createdPost, currentUserId, createdPost.CurrentPlayers));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlaySessionPostDetailResponse>> Update(Guid id, UpdatePlaySessionPostRequest request)
    {
        var currentUserId = currentUserAccessor.GetRequiredUserId(User);
        var post = await dbContext.PlaySessionPosts
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstOrDefaultAsync(playSessionPost => playSessionPost.Id == id);

        if (post is null)
            return NotFound();

        if (post.CreatorUserId != currentUserId)
            return Forbid();

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
        post.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(ToDetailResponse(post, currentUserId, post.CurrentPlayers));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await cancellationService.CancelPlaySessionByHostAsync(
            id,
            currentUserAccessor.GetRequiredUserId(User),
            HttpContext.RequestAborted);

        if (result.Succeeded)
            return NoContent();

        return this.ToErrorResult(result.Error);
    }

    private static PlaySessionPostListItemResponse ToListItemResponse(PlaySessionPost post, string currentUserId, int occupiedSlots)
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

    private static PlaySessionPostDetailResponse ToDetailResponse(PlaySessionPost post, string currentUserId, int occupiedSlots)
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
