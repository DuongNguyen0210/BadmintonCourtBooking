using System.Security.Claims;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/join-requests")]
public sealed class JoinRequestsController(IJoinRequestService joinRequestService) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await joinRequestService.GetMineAsync(GetCurrentUserId(), cancellationToken));
    }

    [HttpPost("{joinRequestId:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(
        Guid joinRequestId,
        [FromServices] IPaymentService paymentService,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.ConfirmPaymentAsync(joinRequestId, GetCurrentUserId(), cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return result.Error?.Code switch
        {
            "JOIN_REQUEST_NOT_FOUND" => NotFound(result.Error),
            "FORBIDDEN" => Forbid(),
            "INSUFFICIENT_BALANCE" => Conflict(result.Error),
            "JOIN_REQUEST_NOT_AWAITING_PAYMENT" => Conflict(result.Error),
            "JOIN_REQUEST_PAYMENT_EXPIRED" => Conflict(result.Error),
            "PLAY_SESSION_NOT_ACTIVE" => Conflict(result.Error),
            "PLAY_SESSION_ALREADY_STARTED" => Conflict(result.Error),
            "PLAY_SESSION_FULL" => Conflict(result.Error),
            "PAYMENT_ALREADY_PROCESSED" => Conflict(result.Error),
            "CONCURRENCY_CONFLICT" => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
