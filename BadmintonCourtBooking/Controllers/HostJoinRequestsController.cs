using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/host/join-requests")]
public sealed class HostJoinRequestsController(
    IJoinRequestService joinRequestService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetForHost([FromQuery] JoinRequestStatus? status, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.GetForHostAsync(
            currentUserAccessor.GetRequiredUserId(User),
            status,
            cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost("{joinRequestId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid joinRequestId, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.ApproveAsync(
            joinRequestId,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return this.ToErrorResult(result.Error);
    }

    [HttpPost("{joinRequestId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid joinRequestId, CancellationToken cancellationToken)
    {
        var result = await joinRequestService.RejectAsync(
            joinRequestId,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return this.ToErrorResult(result.Error);
    }
}
