using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(
    INotificationService notificationService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await notificationService.GetMineAsync(
            currentUserAccessor.GetRequiredUserId(User),
            cancellationToken));
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var result = await notificationService.MarkAsReadAsync(
            currentUserAccessor.GetRequiredUserId(User),
            notificationId,
            cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return this.ToErrorResult(result.Error);
    }
}
