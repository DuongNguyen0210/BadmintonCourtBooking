using System.Security.Claims;

namespace BadmintonCourtBooking.Services;

public interface ICurrentUserAccessor
{
    string GetRequiredUserId(ClaimsPrincipal user);
}
