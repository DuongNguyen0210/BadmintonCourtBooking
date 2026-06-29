using System.Security.Claims;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[Authorize]
[ApiController]
[Route("api/wallet")]
public sealed class WalletController(IWalletService walletService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetWallet(CancellationToken cancellationToken)
    {
        return Ok(await walletService.GetWalletAsync(GetCurrentUserId(), cancellationToken));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(CancellationToken cancellationToken)
    {
        return Ok(await walletService.GetTransactionsAsync(GetCurrentUserId(), cancellationToken));
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
