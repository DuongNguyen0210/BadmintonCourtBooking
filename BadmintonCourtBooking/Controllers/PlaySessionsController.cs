using System.Security.Claims;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.PlaySessions;
using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/play-sessions")]
public sealed class PlaySessionsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlaySessionPostListItemResponse>>> GetFeed()
    {
        var currentUserId = GetCurrentUserId();
        var now = DateTimeOffset.UtcNow;

        var posts = await dbContext.PlaySessionPosts
            .AsNoTracking()
            .Include(post => post.CreatorUser)
            .Where(post =>
                post.Status == PostStatus.Active &&
                post.CurrentPlayers < post.MaxPlayers &&
                post.EndTime > now)
            .OrderBy(post => post.StartTime)
            .ThenByDescending(post => post.CreatedAt)
            .ToListAsync();

        return Ok(posts.Select(post => ToListItemResponse(post, currentUserId)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaySessionPostDetailResponse>> GetById(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var post = await dbContext.PlaySessionPosts
            .AsNoTracking()
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstOrDefaultAsync(playSessionPost => playSessionPost.Id == id);

        if (post is null)
            return NotFound();

        return Ok(ToDetailResponse(post, currentUserId));
    }

    [HttpPost]
    public async Task<ActionResult<PlaySessionPostDetailResponse>> Create(CreatePlaySessionPostRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var post = new PlaySessionPost
        {
            CreatorUserId = currentUserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CourtName = request.CourtName.Trim(),
            CourtAddress = request.CourtAddress.Trim(),
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            PricePerPlayer = request.PricePerPlayer,
            MaxPlayers = request.MaxPlayers,
            CurrentPlayers = request.CurrentPlayers,
            MalePlayers = request.MalePlayers,
            FemalePlayers = request.FemalePlayers,
            ShowMalePlayers = request.ShowMalePlayers,
            ShowFemalePlayers = request.ShowFemalePlayers,
            Status = PostStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PlaySessionPosts.Add(post);
        await dbContext.SaveChangesAsync();

        var createdPost = await dbContext.PlaySessionPosts
            .AsNoTracking()
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstAsync(playSessionPost => playSessionPost.Id == post.Id);

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, ToDetailResponse(createdPost, currentUserId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlaySessionPostDetailResponse>> Update(Guid id, UpdatePlaySessionPostRequest request)
    {
        var currentUserId = GetCurrentUserId();
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
        post.PricePerPlayer = request.PricePerPlayer;
        post.MaxPlayers = request.MaxPlayers;
        post.CurrentPlayers = request.CurrentPlayers;
        post.MalePlayers = request.MalePlayers;
        post.FemalePlayers = request.FemalePlayers;
        post.ShowMalePlayers = request.ShowMalePlayers;
        post.ShowFemalePlayers = request.ShowFemalePlayers;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(ToDetailResponse(post, currentUserId));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var post = await dbContext.PlaySessionPosts.FirstOrDefaultAsync(playSessionPost => playSessionPost.Id == id);

        if (post is null)
            return NotFound();

        if (post.CreatorUserId != currentUserId)
            return Forbid();

        post.Status = PostStatus.Cancelled;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }

    private static PlaySessionPostListItemResponse ToListItemResponse(PlaySessionPost post, string currentUserId)
    {
        return new PlaySessionPostListItemResponse(
            post.Id,
            post.Title,
            post.CourtName,
            post.CourtAddress,
            post.StartTime,
            post.EndTime,
            post.PricePerPlayer,
            post.MaxPlayers,
            post.CurrentPlayers,
            post.ShowMalePlayers ? post.MalePlayers : null,
            post.ShowFemalePlayers ? post.FemalePlayers : null,
            post.Status.ToString(),
            post.CreatorUser.FullName,
            post.CreatorUserId == currentUserId);
    }

    private static PlaySessionPostDetailResponse ToDetailResponse(PlaySessionPost post, string currentUserId)
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
            post.MaxPlayers,
            post.CurrentPlayers,
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
