using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/play-sessions/{playSessionPostId:guid}/join-requests")]
public sealed class PlaySessionJoinRequestsController(
    IJoinRequestService joinRequestService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RequestToJoin(Guid playSessionPostId, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.RequestToJoinAsync(
            playSessionPostId,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        if (result.Succeeded)
            return Created($"/api/join-requests/{result.Value!.Id}", result.Value);

        return this.ToErrorResult(result.Error);
    }
}
