using System.Security.Claims;
using BadmintonCourtBooking.Dtos.Participations;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/participations")]
public sealed class ParticipationsController(ICancellationService cancellationService) : ControllerBase
{
    [HttpPost("{participantId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid participantId,
        CancelParticipationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cancellationService.CancelParticipationAsync(
            participantId,
            GetCurrentUserId(),
            request,
            cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return result.Error?.Code switch
        {
            "PARTICIPANT_NOT_FOUND" => NotFound(result.Error),
            "FORBIDDEN" => Forbid(),
            "WAIVE_REFUND_CONFIRMATION_REQUIRED" => BadRequest(result.Error),
            "PARTICIPANT_ALREADY_CANCELLED" => Conflict(result.Error),
            "PARTICIPATION_ALREADY_CANCELLED" => Conflict(result.Error),
            "PLAY_SESSION_NOT_ACTIVE" => Conflict(result.Error),
            "PLAY_SESSION_ALREADY_STARTED" => Conflict(result.Error),
            "PAYMENT_NOT_FOUND" => Conflict(result.Error),
            "INSUFFICIENT_HELD_BALANCE" => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
