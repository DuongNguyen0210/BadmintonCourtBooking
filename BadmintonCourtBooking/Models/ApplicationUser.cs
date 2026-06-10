using Microsoft.AspNetCore.Identity;

namespace BadmintonCourtBooking.Models;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
