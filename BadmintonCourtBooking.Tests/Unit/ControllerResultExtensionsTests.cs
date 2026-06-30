using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Tests.Unit;

public sealed class ControllerResultExtensionsTests
{
    private readonly TestController controller = new();

    [Theory]
    [InlineData("PLAY_SESSION_NOT_FOUND")]
    [InlineData("JOIN_REQUEST_NOT_FOUND")]
    [InlineData("PARTICIPANT_NOT_FOUND")]
    [InlineData("NOTIFICATION_NOT_FOUND")]
    public void ToErrorResult_returns_not_found_for_missing_resources(string code)
    {
        var result = controller.ToErrorResult(new ServiceError(code, "Missing."));

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Theory]
    [InlineData("FORBIDDEN")]
    [InlineData("UNAUTHORIZED_NOTIFICATION_ACCESS")]
    public void ToErrorResult_returns_forbid_for_unauthorized_resource_access(string code)
    {
        var result = controller.ToErrorResult(new ServiceError(code, "Forbidden."));

        Assert.IsType<ForbidResult>(result);
    }

    [Theory]
    [InlineData("DUPLICATE_JOIN_REQUEST")]
    [InlineData("PLAY_SESSION_FULL")]
    [InlineData("INSUFFICIENT_BALANCE")]
    [InlineData("PAYMENT_ALREADY_PROCESSED")]
    [InlineData("PARTICIPATION_ALREADY_CANCELLED")]
    public void ToErrorResult_returns_conflict_for_state_conflicts(string code)
    {
        var result = controller.ToErrorResult(new ServiceError(code, "Conflict."));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void ToErrorResult_returns_bad_request_for_unknown_error()
    {
        var result = controller.ToErrorResult(new ServiceError("UNKNOWN", "Unknown."));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private sealed class TestController : ControllerBase;
}
