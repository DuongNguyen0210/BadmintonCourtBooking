using System.Security.Claims;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/host/join-requests")]
public sealed class HostJoinRequestsController(IJoinRequestService joinRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetForHost([FromQuery] JoinRequestStatus? status, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.GetForHostAsync(GetCurrentUserId(), status, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost("{joinRequestId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid joinRequestId, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.ApproveAsync(joinRequestId, GetCurrentUserId(), cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return ToErrorResponse(result.Error);
    }

    [HttpPost("{joinRequestId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid joinRequestId, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.RejectAsync(joinRequestId, GetCurrentUserId(), cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return ToErrorResponse(result.Error);
    }

    private IActionResult ToErrorResponse(ServiceError? error)
    {
        return error?.Code switch
        {
            "JOIN_REQUEST_NOT_FOUND" => NotFound(error),
            "FORBIDDEN" => Forbid(),
            "JOIN_REQUEST_NOT_PENDING" => Conflict(error),
            "PLAY_SESSION_NOT_ACTIVE" => Conflict(error),
            "PLAY_SESSION_ALREADY_STARTED" => Conflict(error),
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
