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
    IWalletService walletService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpPost("top-up")]
    public async Task<IActionResult> TopUp(DevelopmentTopUpRequest request, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return NotFound(new ServiceError("DEVELOPMENT_ENDPOINT_DISABLED", "Development wallet top-up is not available."));

        var result = await walletService.TopUpDevelopmentAsync(
            currentUserAccessor.GetRequiredUserId(User),
            request.AmountVnd,
            cancellationToken);

        if (result.Succeeded)
            return Ok(result.Value);

        return this.ToErrorResult(result.Error);
    }
}
