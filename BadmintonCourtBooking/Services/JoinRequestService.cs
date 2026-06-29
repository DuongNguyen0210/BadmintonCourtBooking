using System.Data;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.JoinRequests;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BadmintonCourtBooking.Services;

public sealed class JoinRequestService(
    ApplicationDbContext dbContext,
    IClock clock,
    IOptions<PaymentOptions> paymentOptions) : IJoinRequestService
{
    private static readonly JoinRequestStatus[] ActiveRequestStatuses =
    [
        JoinRequestStatus.PendingHostApproval,
        JoinRequestStatus.AwaitingPayment,
        JoinRequestStatus.Joined
    ];

    public async Task<ServiceResult<JoinRequestResponse>> RequestToJoinAsync(
        Guid playSessionPostId,
        string guestUserId,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var post = await dbContext.PlaySessionPosts
            .Include(playSessionPost => playSessionPost.CreatorUser)
            .FirstOrDefaultAsync(playSessionPost => playSessionPost.Id == playSessionPostId, cancellationToken);

        if (post is null)
            return ServiceResult<JoinRequestResponse>.Failure("PLAY_SESSION_NOT_FOUND", "Play session was not found.");

        var validationError = await ValidateGuestCanRequestAsync(post, guestUserId, now, cancellationToken);
        if (validationError is not null)
            return ServiceResult<JoinRequestResponse>.Failure(validationError.Code, validationError.Message);

        var joinRequest = PlaySessionJoinRequest.Create(playSessionPostId, guestUserId, now);
        dbContext.PlaySessionJoinRequests.Add(joinRequest);
        dbContext.Notifications.Add(Notification.Create(
            post.CreatorUserId,
            NotificationType.JoinRequested,
            "New join request",
            $"{guestUserId} requested to join {post.Title}.",
            now,
            joinRequest.Id.ToString()));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ServiceResult<JoinRequestResponse>.Failure("DUPLICATE_JOIN_REQUEST", "You already have an active request for this session.");
        }

        var created = await GetRequestByIdQuery()
            .FirstAsync(request => request.Id == joinRequest.Id, cancellationToken);

        return ServiceResult<JoinRequestResponse>.Success(ToResponse(created));
    }

    public async Task<IReadOnlyList<JoinRequestResponse>> GetMineAsync(
        string guestUserId,
        CancellationToken cancellationToken)
    {
        return await GetRequestByIdQuery()
            .Where(request => request.GuestUserId == guestUserId)
            .OrderByDescending(request => request.RequestedAtUtc)
            .Select(request => ToResponse(request))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<IReadOnlyList<JoinRequestResponse>>> GetForHostAsync(
        string hostUserId,
        JoinRequestStatus? status,
        CancellationToken cancellationToken)
    {
        var query = GetRequestByIdQuery()
            .Where(request => request.PlaySessionPost.CreatorUserId == hostUserId);

        if (status is not null)
            query = query.Where(request => request.Status == status);

        var requests = await query
            .OrderByDescending(request => request.RequestedAtUtc)
            .Select(request => ToResponse(request))
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<JoinRequestResponse>>.Success(requests);
    }

    public async Task<ServiceResult<JoinRequestResponse>> ApproveAsync(
        Guid joinRequestId,
        string hostUserId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var request = await GetRequestForUpdateAsync(joinRequestId, cancellationToken);
        if (request is null)
            return ServiceResult<JoinRequestResponse>.Failure("JOIN_REQUEST_NOT_FOUND", "Join request was not found.");

        if (request.PlaySessionPost.CreatorUserId != hostUserId)
            return ServiceResult<JoinRequestResponse>.Failure("FORBIDDEN", "Only the host can approve this request.");

        var now = clock.UtcNow;
        var validationError = await ValidateSessionCanReserveSlotAsync(request.PlaySessionPost, now, cancellationToken);
        if (validationError is not null)
            return ServiceResult<JoinRequestResponse>.Failure(validationError.Code, validationError.Message);

        try
        {
            request.Approve(hostUserId, now, now.AddMinutes(paymentOptions.Value.PaymentWindowMinutes));
        }
        catch (DomainException exception)
        {
            return ServiceResult<JoinRequestResponse>.Failure(exception.Code, exception.Message);
        }

        dbContext.Notifications.Add(Notification.Create(
            request.GuestUserId,
            NotificationType.PaymentRequired,
            "Payment required",
            $"Your request for {request.PlaySessionPost.Title} was approved. Please pay before the deadline.",
            now,
            request.Id.ToString()));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<JoinRequestResponse>.Success(ToResponse(request));
    }

    public async Task<ServiceResult<JoinRequestResponse>> RejectAsync(
        Guid joinRequestId,
        string hostUserId,
        CancellationToken cancellationToken)
    {
        var request = await GetRequestForUpdateAsync(joinRequestId, cancellationToken);
        if (request is null)
            return ServiceResult<JoinRequestResponse>.Failure("JOIN_REQUEST_NOT_FOUND", "Join request was not found.");

        if (request.PlaySessionPost.CreatorUserId != hostUserId)
            return ServiceResult<JoinRequestResponse>.Failure("FORBIDDEN", "Only the host can reject this request.");

        var now = clock.UtcNow;

        try
        {
            request.Reject(hostUserId, now);
        }
        catch (DomainException exception)
        {
            return ServiceResult<JoinRequestResponse>.Failure(exception.Code, exception.Message);
        }

        dbContext.Notifications.Add(Notification.Create(
            request.GuestUserId,
            NotificationType.JoinRejected,
            "Join request rejected",
            $"Your request for {request.PlaySessionPost.Title} was rejected.",
            now,
            request.Id.ToString()));

        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<JoinRequestResponse>.Success(ToResponse(request));
    }

    private async Task<ServiceError?> ValidateGuestCanRequestAsync(
        PlaySessionPost post,
        string guestUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (post.CreatorUserId == guestUserId)
            return new ServiceError("HOST_CANNOT_JOIN_OWN_SESSION", "Host cannot join their own play session.");

        if (post.Status != PostStatus.Active)
            return new ServiceError("PLAY_SESSION_NOT_ACTIVE", "Play session is not active.");

        if (post.StartTime <= now)
            return new ServiceError("PLAY_SESSION_ALREADY_STARTED", "Play session has already started.");

        if (await HasActiveRequestAsync(post.Id, guestUserId, cancellationToken))
            return new ServiceError("DUPLICATE_JOIN_REQUEST", "You already have an active request for this session.");

        if (await HasActiveParticipationAsync(post.Id, guestUserId, cancellationToken))
            return new ServiceError("ALREADY_PARTICIPANT", "You already joined this session.");

        if (await GetOccupiedSlotsAsync(post, now, cancellationToken) >= post.MaxPlayers)
            return new ServiceError("PLAY_SESSION_FULL", "Play session is full.");

        return null;
    }

    private async Task<ServiceError?> ValidateSessionCanReserveSlotAsync(
        PlaySessionPost post,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (post.Status != PostStatus.Active)
            return new ServiceError("PLAY_SESSION_NOT_ACTIVE", "Play session is not active.");

        if (post.StartTime <= now)
            return new ServiceError("PLAY_SESSION_ALREADY_STARTED", "Play session has already started.");

        if (await GetOccupiedSlotsAsync(post, now, cancellationToken) >= post.MaxPlayers)
            return new ServiceError("PLAY_SESSION_FULL", "Play session is full.");

        return null;
    }

    private Task<bool> HasActiveRequestAsync(Guid playSessionPostId, string guestUserId, CancellationToken cancellationToken)
    {
        return dbContext.PlaySessionJoinRequests.AnyAsync(
            request =>
                request.PlaySessionPostId == playSessionPostId &&
                request.GuestUserId == guestUserId &&
                ActiveRequestStatuses.Contains(request.Status),
            cancellationToken);
    }

    private Task<bool> HasActiveParticipationAsync(Guid playSessionPostId, string guestUserId, CancellationToken cancellationToken)
    {
        return dbContext.PlaySessionParticipants.AnyAsync(
            participant =>
                participant.PlaySessionPostId == playSessionPostId &&
                participant.UserId == guestUserId &&
                participant.Status == ParticipantStatus.Active,
            cancellationToken);
    }

    private async Task<int> GetOccupiedSlotsAsync(
        PlaySessionPost post,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var activeParticipants = await dbContext.PlaySessionParticipants.CountAsync(
            participant =>
                participant.PlaySessionPostId == post.Id &&
                participant.Status == ParticipantStatus.Active,
            cancellationToken);

        var heldRequests = await dbContext.PlaySessionJoinRequests.CountAsync(
            request =>
                request.PlaySessionPostId == post.Id &&
                request.Status == JoinRequestStatus.AwaitingPayment &&
                request.PaymentDueAtUtc > now,
            cancellationToken);

        return post.CurrentPlayers + activeParticipants + heldRequests;
    }

    private Task<PlaySessionJoinRequest?> GetRequestForUpdateAsync(Guid joinRequestId, CancellationToken cancellationToken)
    {
        return GetRequestByIdQuery()
            .FirstOrDefaultAsync(request => request.Id == joinRequestId, cancellationToken);
    }

    private IQueryable<PlaySessionJoinRequest> GetRequestByIdQuery()
    {
        return dbContext.PlaySessionJoinRequests
            .Include(request => request.PlaySessionPost)
            .Include(request => request.GuestUser);
    }

    private static JoinRequestResponse ToResponse(PlaySessionJoinRequest request)
    {
        return new JoinRequestResponse(
            request.Id,
            request.PlaySessionPostId,
            request.PlaySessionPost.Title,
            request.PlaySessionPost.CourtName,
            request.GuestUserId,
            request.GuestUser.FullName,
            request.Status.ToString(),
            request.PlaySessionPost.PricePerPlayerVnd,
            request.RequestedAtUtc,
            request.ReviewedAtUtc,
            request.PaymentDueAtUtc,
            request.PaidAtUtc,
            request.CancelledAtUtc);
    }
}
