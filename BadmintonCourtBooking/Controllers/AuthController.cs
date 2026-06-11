using BadmintonCourtBooking.Dtos;
using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : ControllerBase
{
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

        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
            return Ok(new AuthResponse(true, "Registration successful.", ToUserResponse(user)));
        var message = string.Join(" ", result.Errors.Select(error => error.Description));
        return BadRequest(new AuthResponse(false, message, null));

    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null)
            return Unauthorized(new AuthResponse(false, "Invalid email or password.", null));

        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized(new AuthResponse(false, "Invalid email or password.", null));

        return Ok(new AuthResponse(true, "Login successful.", ToUserResponse(user)));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok(new AuthResponse(true, "Logout successful.", null));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        return Ok(ToUserResponse(user));
    }

    private static UserResponse ToUserResponse(ApplicationUser user)
    {
        return new UserResponse(user.Id, user.Email ?? string.Empty, user.FullName);
    }
}
