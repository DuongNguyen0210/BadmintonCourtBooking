using System.Data;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.JoinRequests;
using BadmintonCourtBooking.Dtos.Payments;
using BadmintonCourtBooking.Dtos.Wallet;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public sealed class PaymentService(
    ApplicationDbContext dbContext,
    IClock clock,
    IPlaySessionAvailabilityService availabilityService,
    IWalletAccountingService walletAccountingService) : IPaymentService
{
    public async Task<ServiceResult<ConfirmPaymentResponse>> ConfirmPaymentAsync(
        Guid joinRequestId,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var request = await dbContext.PlaySessionJoinRequests
            .Include(joinRequest => joinRequest.PlaySessionPost)
            .Include(joinRequest => joinRequest.GuestUser)
            .FirstOrDefaultAsync(joinRequest => joinRequest.Id == joinRequestId, cancellationToken);

        if (request is null)
            return ServiceResult<ConfirmPaymentResponse>.Failure("JOIN_REQUEST_NOT_FOUND", "Join request was not found.");

        if (request.GuestUserId != userId)
            return ServiceResult<ConfirmPaymentResponse>.Failure("FORBIDDEN", "You can only pay for your own join request.");

        var existingParticipant = await dbContext.PlaySessionParticipants.FirstOrDefaultAsync(
            participant =>
                participant.JoinRequestId == joinRequestId &&
                participant.UserId == userId &&
                participant.Status == ParticipantStatus.Active,
            cancellationToken);

        if (request.Status == JoinRequestStatus.Joined && existingParticipant is not null)
        {
            var existingWallet = await walletAccountingService.GetOrCreateWalletAsync(userId, cancellationToken);
            return ServiceResult<ConfirmPaymentResponse>.Success(ToResponse(existingParticipant.Id, request, existingWallet));
        }

        var now = clock.UtcNow;
        var validationError = await ValidateCanPayAsync(request, now, cancellationToken);
        if (validationError is not null)
        {
            if (validationError.Code == "JOIN_REQUEST_PAYMENT_EXPIRED")
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            return ServiceResult<ConfirmPaymentResponse>.Failure(validationError.Code, validationError.Message);
        }

        var amountVnd = request.PlaySessionPost.PricePerPlayerVnd;
        if (amountVnd <= 0)
            return ServiceResult<ConfirmPaymentResponse>.Failure("INVALID_SESSION_PRICE", "Play session price must be greater than zero.");

        var wallet = await walletAccountingService.GetOrCreateWalletAsync(userId, cancellationToken);
        if (wallet.AvailableBalanceVnd < amountVnd)
            return ServiceResult<ConfirmPaymentResponse>.Failure("INSUFFICIENT_BALANCE", "Available balance is not enough.");

        var participant = PlaySessionParticipant.Create(
            request.PlaySessionPostId,
            userId,
            request.Id,
            now);

        request.MarkAsPaid(now);

        dbContext.PlaySessionParticipants.Add(participant);
        walletAccountingService.HoldEscrow(
            wallet,
            amountVnd,
            now,
            request.PlaySessionPost.CreatorUserId,
            request.PlaySessionPostId,
            request.Id,
            $"Escrow hold for {request.PlaySessionPost.Title}",
            $"confirm-payment:{request.Id}");

        dbContext.Notifications.Add(Notification.Create(
            userId,
            NotificationType.PaymentCompleted,
            "Payment completed",
            $"You joined {request.PlaySessionPost.Title}.",
            now,
            request.Id.ToString()));

        dbContext.Notifications.Add(Notification.Create(
            request.PlaySessionPost.CreatorUserId,
            NotificationType.ParticipantJoined,
            "Participant joined",
            $"{request.GuestUser.FullName} paid and joined {request.PlaySessionPost.Title}.",
            now,
            request.Id.ToString()));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<ConfirmPaymentResponse>.Failure("CONCURRENCY_CONFLICT", "Payment could not be completed because the wallet changed. Please retry.");
        }
        catch (DbUpdateException)
        {
            return ServiceResult<ConfirmPaymentResponse>.Failure("PAYMENT_ALREADY_PROCESSED", "Payment was already processed.");
        }

        return ServiceResult<ConfirmPaymentResponse>.Success(ToResponse(participant.Id, request, wallet));
    }

    private async Task<ServiceError?> ValidateCanPayAsync(
        PlaySessionJoinRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (request.Status != JoinRequestStatus.AwaitingPayment)
            return new ServiceError("JOIN_REQUEST_NOT_AWAITING_PAYMENT", "Join request is not awaiting payment.");

        if (request.PaymentDueAtUtc is null)
            return new ServiceError("PAYMENT_DUE_AT_MISSING", "Payment deadline is missing.");

        if (request.PaymentDueAtUtc < now)
        {
            request.Expire(now);
            return new ServiceError("JOIN_REQUEST_PAYMENT_EXPIRED", "Payment deadline has passed.");
        }

        if (request.PlaySessionPost.Status != PostStatus.Active)
            return new ServiceError("PLAY_SESSION_NOT_ACTIVE", "Play session is not active.");

        if (request.PlaySessionPost.StartTime <= now)
            return new ServiceError("PLAY_SESSION_ALREADY_STARTED", "Play session has already started.");

        var occupiedSlotsExcludingThisRequest = await availabilityService.GetOccupiedSlotsAsync(
            request.PlaySessionPost,
            now,
            cancellationToken,
            request.Id);

        if (occupiedSlotsExcludingThisRequest >= request.PlaySessionPost.MaxPlayers)
            return new ServiceError("PLAY_SESSION_FULL", "Play session is full.");

        return null;
    }

    private static ConfirmPaymentResponse ToResponse(Guid participantId, PlaySessionJoinRequest request, Wallet wallet)
    {
        return new ConfirmPaymentResponse(
            participantId,
            new JoinRequestResponse(
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
                request.CancelledAtUtc),
            new WalletResponse(wallet.AvailableBalanceVnd, wallet.HeldBalanceVnd));
    }
}
