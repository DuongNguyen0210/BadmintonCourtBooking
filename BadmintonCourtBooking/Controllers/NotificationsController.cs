using System.Security.Claims;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await notificationService.GetMineAsync(GetCurrentUserId(), cancellationToken));
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var result = await notificationService.MarkAsReadAsync(GetCurrentUserId(), notificationId, cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return result.Error?.Code switch
        {
            "NOTIFICATION_NOT_FOUND" => NotFound(result.Error),
            "UNAUTHORIZED_NOTIFICATION_ACCESS" => Forbid(),
            _ => BadRequest(result.Error)
        };
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
