using BadmintonCourtBooking.Dtos;
using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            return BadRequest(new AuthResponse(false, message, null));
        }

        return Ok(new AuthResponse(
            true,
            "Registration successful.",
            ToUserResponse(user)));
    }

    private static UserResponse ToUserResponse(ApplicationUser user)
    {
        return new UserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName);
    }
}
