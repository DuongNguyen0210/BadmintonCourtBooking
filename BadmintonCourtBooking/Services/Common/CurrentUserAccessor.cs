using System.Security.Claims;

namespace BadmintonCourtBooking.Services;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    public string GetRequiredUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
