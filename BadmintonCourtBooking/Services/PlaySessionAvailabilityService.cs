using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class PlaySessionAvailabilityService(ApplicationDbContext dbContext) : IPlaySessionAvailabilityService
{
    public async Task<int> GetOccupiedSlotsAsync(
        PlaySessionPost post,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var activeParticipants = await dbContext.PlaySessionParticipants.CountAsync(
            participant =>
                participant.PlaySessionPostId == post.Id &&
                participant.Status == ParticipantStatus.Active,
            cancellationToken);

        var heldJoinRequests = await dbContext.PlaySessionJoinRequests.CountAsync(
            request =>
                request.PlaySessionPostId == post.Id &&
                request.Status == JoinRequestStatus.AwaitingPayment &&
                request.PaymentDueAtUtc > now,
            cancellationToken);

        return post.CurrentPlayers + activeParticipants + heldJoinRequests;
    }

    public async Task<bool> IsVisibleOnFeedAsync(
        PlaySessionPost post,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (post.Status != PostStatus.Active)
            return false;

        if (post.StartTime <= now)
            return false;

        return await GetOccupiedSlotsAsync(post, now, cancellationToken) < post.MaxPlayers;
    }
}
