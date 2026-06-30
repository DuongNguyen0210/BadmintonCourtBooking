using BadmintonCourtBooking.Dtos.Participations;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/participations")]
public sealed class ParticipationsController(
    IParticipationCancellationService cancellationService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpPost("{participantId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid participantId,
        CancelParticipationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cancellationService.CancelParticipationAsync(
            participantId,
            currentUserAccessor.GetRequiredUserId(User),
            request,
            cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return this.ToErrorResult(result.Error);
    }
}
