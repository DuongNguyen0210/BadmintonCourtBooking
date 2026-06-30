using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/join-requests")]
public sealed class JoinRequestsController(
    IJoinRequestService joinRequestService,
    IPaymentService paymentService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await joinRequestService.GetMineAsync(
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken));
    }

    [HttpPost("{joinRequestId:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(
        Guid joinRequestId,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.ConfirmPaymentAsync(
            joinRequestId,
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return this.ToErrorResult(result.Error);
    }
}
