using BadmintonCourtBooking.Dtos.PlaySessions;
using BadmintonCourtBooking.Features.PlaySessions;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/play-sessions")]
public sealed class PlaySessionsController(
    IPlaySessionPostService playSessionPostService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlaySessionPostListItemResponse>>> GetFeed(CancellationToken cancellationToken)
    {
        return Ok(await playSessionPostService.GetFeedAsync(
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await playSessionPostService.GetByIdAsync(
            id,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        return result.Succeeded
            ? Ok(result.Value)
            : this.ToErrorResult(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePlaySessionPostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await playSessionPostService.CreateAsync(
            request,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.ToErrorResult(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePlaySessionPostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await playSessionPostService.UpdateAsync(
            id,
            request,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        return result.Succeeded
            ? Ok(result.Value)
            : this.ToErrorResult(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await playSessionPostService.CancelAsync(
            id,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        if (result.Succeeded)
            return NoContent();

        return this.ToErrorResult(result.Error);
    }
}
