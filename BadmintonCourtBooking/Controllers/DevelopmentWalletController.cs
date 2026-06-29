using System.Security.Claims;
using BadmintonCourtBooking.Dtos.Development;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/development/wallet")]
public sealed class DevelopmentWalletController(
    IHostEnvironment environment,
    IWalletService walletService) : ControllerBase
{
    [HttpPost("top-up")]
    public async Task<IActionResult> TopUp(DevelopmentTopUpRequest request, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return NotFound(new ServiceError("DEVELOPMENT_ENDPOINT_DISABLED", "Development wallet top-up is not available."));

        var result = await walletService.TopUpDevelopmentAsync(GetCurrentUserId(), request.AmountVnd, cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
