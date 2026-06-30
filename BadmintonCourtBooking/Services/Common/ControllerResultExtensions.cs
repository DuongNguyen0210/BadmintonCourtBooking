using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Services;

public static class ControllerResultExtensions
{
    private static readonly HashSet<string> NotFoundCodes =
    [
        "PLAY_SESSION_NOT_FOUND",
        "JOIN_REQUEST_NOT_FOUND",
        "PARTICIPANT_NOT_FOUND",
        "NOTIFICATION_NOT_FOUND"
    ];

    private static readonly HashSet<string> ForbiddenCodes =
    [
        "FORBIDDEN",
        "UNAUTHORIZED_NOTIFICATION_ACCESS"
    ];

    private static readonly HashSet<string> ConflictCodes =
    [
        "HOST_CANNOT_JOIN_OWN_SESSION",
        "PLAY_SESSION_NOT_ACTIVE",
        "PLAY_SESSION_ALREADY_STARTED",
        "DUPLICATE_JOIN_REQUEST",
        "ALREADY_PARTICIPANT",
        "PLAY_SESSION_FULL",
        "JOIN_REQUEST_NOT_PENDING",
        "INSUFFICIENT_BALANCE",
        "JOIN_REQUEST_NOT_AWAITING_PAYMENT",
        "JOIN_REQUEST_PAYMENT_EXPIRED",
        "PAYMENT_ALREADY_PROCESSED",
        "CONCURRENCY_CONFLICT",
        "PARTICIPANT_ALREADY_CANCELLED",
        "PARTICIPATION_ALREADY_CANCELLED",
        "PAYMENT_NOT_FOUND",
        "INSUFFICIENT_HELD_BALANCE"
    ];

    public static IActionResult ToErrorResult(this ControllerBase controller, ServiceError? error)
    {
        if (error is null)
            return controller.BadRequest();

        if (NotFoundCodes.Contains(error.Code))
            return controller.NotFound(error);

        if (ForbiddenCodes.Contains(error.Code))
            return controller.Forbid();

        if (ConflictCodes.Contains(error.Code))
            return controller.Conflict(error);

        return controller.BadRequest(error);
    }
}
