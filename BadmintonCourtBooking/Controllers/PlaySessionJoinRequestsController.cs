using System.Security.Claims;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/play-sessions/{playSessionPostId:guid}/join-requests")]
public sealed class PlaySessionJoinRequestsController(IJoinRequestService joinRequestService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RequestToJoin(Guid playSessionPostId, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.RequestToJoinAsync(playSessionPostId, GetCurrentUserId(), cancellationToken);

        if (result.Succeeded)
            return Created($"/api/join-requests/{result.Value!.Id}", result.Value);

        return ToErrorResponse(result.Error);
    }

    private IActionResult ToErrorResponse(ServiceError? error)
    {
        return error?.Code switch
        {
            "PLAY_SESSION_NOT_FOUND" => NotFound(error),
            "HOST_CANNOT_JOIN_OWN_SESSION" => Conflict(error),
            "PLAY_SESSION_NOT_ACTIVE" => Conflict(error),
            "PLAY_SESSION_ALREADY_STARTED" => Conflict(error),
            "DUPLICATE_JOIN_REQUEST" => Conflict(error),
            "ALREADY_PARTICIPANT" => Conflict(error),
            "PLAY_SESSION_FULL" => Conflict(error),
            _ => BadRequest(error)
        };
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
