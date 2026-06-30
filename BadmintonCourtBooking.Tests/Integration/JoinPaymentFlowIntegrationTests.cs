using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Dtos.Participations;
using BadmintonCourtBooking.Features.PlaySessions;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Options;
using BadmintonCourtBooking.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BadmintonCourtBooking.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class JoinPaymentFlowIntegrationTests(PostgresTestContainerFixture fixture)
{
    private static readonly SystemClock Clock = new();

    [Fact]
    public async Task Guest_can_request_host_approval_and_pay_to_join()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 2, priceVnd: 100_000);
        var services = CreateServices(dbContext);

        var requestResult = await services.JoinRequests.RequestToJoinAsync(seed.Post.Id, seed.Guest.Id, CancellationToken.None);
        var approveResult = await services.JoinRequests.ApproveAsync(requestResult.Value!.Id, seed.Host.Id, CancellationToken.None);
        await services.Wallets.TopUpDevelopmentAsync(seed.Guest.Id, 150_000, CancellationToken.None);

        var paymentResult = await services.Payments.ConfirmPaymentAsync(approveResult.Value!.Id, seed.Guest.Id, CancellationToken.None);

        Assert.True(paymentResult.Succeeded);
        Assert.Equal(50_000, paymentResult.Value!.Wallet.AvailableBalanceVnd);
        Assert.Equal(100_000, paymentResult.Value.Wallet.HeldBalanceVnd);
        Assert.Equal(JoinRequestStatus.Joined.ToString(), paymentResult.Value.JoinRequest.Status);
        Assert.Equal(1, await dbContext.PlaySessionParticipants.CountAsync(CancellationToken.None));
        Assert.Equal(1, await dbContext.WalletTransactions.CountAsync(
            transaction => transaction.Type == WalletTransactionType.EscrowHold,
            CancellationToken.None));
    }

    [Fact]
    public async Task Duplicate_active_join_request_is_rejected()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 2, priceVnd: 100_000);
        var services = CreateServices(dbContext);

        var firstResult = await services.JoinRequests.RequestToJoinAsync(seed.Post.Id, seed.Guest.Id, CancellationToken.None);
        var duplicateResult = await services.JoinRequests.RequestToJoinAsync(seed.Post.Id, seed.Guest.Id, CancellationToken.None);

        Assert.True(firstResult.Succeeded);
        Assert.False(duplicateResult.Succeeded);
        Assert.Equal("DUPLICATE_JOIN_REQUEST", duplicateResult.Error!.Code);
    }

    [Fact]
    public async Task Different_host_cannot_approve_join_request()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 2, priceVnd: 100_000);
        var otherHost = CreateUser("other-host@test.local", "Other Host");
        dbContext.Users.Add(otherHost);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var services = CreateServices(dbContext);

        var requestResult = await services.JoinRequests.RequestToJoinAsync(seed.Post.Id, seed.Guest.Id, CancellationToken.None);
        var approveResult = await services.JoinRequests.ApproveAsync(requestResult.Value!.Id, otherHost.Id, CancellationToken.None);

        Assert.False(approveResult.Succeeded);
        Assert.Equal("FORBIDDEN", approveResult.Error!.Code);
    }

    [Fact]
    public async Task Insufficient_balance_does_not_create_participant_or_escrow()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 2, priceVnd: 100_000);
        var services = CreateServices(dbContext);

        var requestResult = await services.JoinRequests.RequestToJoinAsync(seed.Post.Id, seed.Guest.Id, CancellationToken.None);
        var approveResult = await services.JoinRequests.ApproveAsync(requestResult.Value!.Id, seed.Host.Id, CancellationToken.None);

        var paymentResult = await services.Payments.ConfirmPaymentAsync(approveResult.Value!.Id, seed.Guest.Id, CancellationToken.None);

        Assert.False(paymentResult.Succeeded);
        Assert.Equal("INSUFFICIENT_BALANCE", paymentResult.Error!.Code);
        Assert.Equal(0, await dbContext.PlaySessionParticipants.CountAsync(CancellationToken.None));
        Assert.Equal(0, await dbContext.WalletTransactions.CountAsync(
            transaction => transaction.Type == WalletTransactionType.EscrowHold,
            CancellationToken.None));
    }

    [Fact]
    public async Task Standard_cancellation_refunds_guest_and_pays_host_fee()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 2, priceVnd: 100_000);
        var services = CreateServices(dbContext);
        var participantId = await JoinWithPaymentAsync(services, seed, topUpAmountVnd: 100_000);

        var result = await services.ParticipationCancellations.CancelParticipationAsync(
            participantId,
            seed.Guest.Id,
            new CancelParticipationRequest
            {
                RefundChoice = CancellationRefundChoice.StandardRefund,
                Reason = "Cannot join"
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(90_000, result.Value!.RefundAmountVnd);
        Assert.Equal(10_000, result.Value.CancellationFeeVnd);

        var guestWallet = await dbContext.Wallets.SingleAsync(wallet => wallet.UserId == seed.Guest.Id, CancellationToken.None);
        var hostWallet = await dbContext.Wallets.SingleAsync(wallet => wallet.UserId == seed.Host.Id, CancellationToken.None);
        Assert.Equal(90_000, guestWallet.AvailableBalanceVnd);
        Assert.Equal(0, guestWallet.HeldBalanceVnd);
        Assert.Equal(10_000, hostWallet.AvailableBalanceVnd);
        Assert.Equal(ParticipantStatus.Cancelled, await dbContext.PlaySessionParticipants
            .Where(participant => participant.Id == participantId)
            .Select(participant => participant.Status)
            .SingleAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Waive_refund_requires_exact_confirmation_text()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 2, priceVnd: 100_000);
        var services = CreateServices(dbContext);
        var participantId = await JoinWithPaymentAsync(services, seed, topUpAmountVnd: 100_000);

        var result = await services.ParticipationCancellations.CancelParticipationAsync(
            participantId,
            seed.Guest.Id,
            new CancelParticipationRequest
            {
                RefundChoice = CancellationRefundChoice.WaiveRefund,
                WaiveRefundConfirmation = "wrong"
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("WAIVE_REFUND_CONFIRMATION_REQUIRED", result.Error!.Code);
        Assert.Equal(ParticipantStatus.Active, await dbContext.PlaySessionParticipants
            .Where(participant => participant.Id == participantId)
            .Select(participant => participant.Status)
            .SingleAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Host_cancels_session_refunds_full_payment_to_guest()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 2, priceVnd: 100_000);
        var services = CreateServices(dbContext);
        var participantId = await JoinWithPaymentAsync(services, seed, topUpAmountVnd: 100_000);

        var result = await services.HostCancellations.CancelPlaySessionByHostAsync(
            seed.Post.Id,
            seed.Host.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var guestWallet = await dbContext.Wallets.SingleAsync(wallet => wallet.UserId == seed.Guest.Id, CancellationToken.None);
        Assert.Equal(100_000, guestWallet.AvailableBalanceVnd);
        Assert.Equal(0, guestWallet.HeldBalanceVnd);
        Assert.Equal(PostStatus.Cancelled, await dbContext.PlaySessionPosts
            .Where(post => post.Id == seed.Post.Id)
            .Select(post => post.Status)
            .SingleAsync(CancellationToken.None));
        Assert.Equal(ParticipantStatus.Cancelled, await dbContext.PlaySessionParticipants
            .Where(participant => participant.Id == participantId)
            .Select(participant => participant.Status)
            .SingleAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Full_post_reappears_on_feed_after_participant_cancels()
    {
        await fixture.ResetDatabaseAsync();
        await using var dbContext = fixture.CreateDbContext();
        var seed = await SeedPlaySessionAsync(dbContext, maxPlayers: 1, priceVnd: 100_000);
        var services = CreateServices(dbContext);
        var participantId = await JoinWithPaymentAsync(services, seed, topUpAmountVnd: 100_000);

        var feedWhileFull = await services.PlaySessions.GetFeedAsync(seed.Guest.Id, CancellationToken.None);

        await services.ParticipationCancellations.CancelParticipationAsync(
            participantId,
            seed.Guest.Id,
            new CancelParticipationRequest
            {
                RefundChoice = CancellationRefundChoice.StandardRefund
            },
            CancellationToken.None);
        var feedAfterCancellation = await services.PlaySessions.GetFeedAsync(seed.Guest.Id, CancellationToken.None);

        Assert.DoesNotContain(feedWhileFull, post => post.Id == seed.Post.Id);
        Assert.Contains(feedAfterCancellation, post => post.Id == seed.Post.Id);
    }

    private static async Task<Guid> JoinWithPaymentAsync(
        TestServices services,
        TestSeed seed,
        long topUpAmountVnd)
    {
        var requestResult = await services.JoinRequests.RequestToJoinAsync(seed.Post.Id, seed.Guest.Id, CancellationToken.None);
        var approveResult = await services.JoinRequests.ApproveAsync(requestResult.Value!.Id, seed.Host.Id, CancellationToken.None);
        await services.Wallets.TopUpDevelopmentAsync(seed.Guest.Id, topUpAmountVnd, CancellationToken.None);
        var paymentResult = await services.Payments.ConfirmPaymentAsync(approveResult.Value!.Id, seed.Guest.Id, CancellationToken.None);

        Assert.True(paymentResult.Succeeded);
        return paymentResult.Value!.ParticipantId;
    }

    private static TestServices CreateServices(ApplicationDbContext dbContext)
    {
        var paymentOptions = Microsoft.Extensions.Options.Options.Create(new PaymentOptions());
        var cancellationPolicy = new CancellationPolicy(new CancellationPolicyOptions());
        var walletAccounting = new WalletAccountingService(dbContext, Clock);
        var notifications = new NotificationService(dbContext, Clock);
        var availability = new PlaySessionAvailabilityService(dbContext);
        var wallets = new WalletService(dbContext, Clock, walletAccounting);
        var joinRequests = new JoinRequestService(dbContext, Clock, paymentOptions, availability, notifications);
        var payments = new PaymentService(dbContext, Clock, availability, walletAccounting, notifications);
        var participationCancellations = new ParticipationCancellationService(
            dbContext,
            Clock,
            cancellationPolicy,
            walletAccounting,
            notifications);
        var hostCancellations = new HostPlaySessionCancellationService(
            dbContext,
            Clock,
            walletAccounting,
            notifications);
        var playSessions = new PlaySessionPostService(
            dbContext,
            hostCancellations,
            availability,
            Clock);

        return new TestServices(
            joinRequests,
            payments,
            wallets,
            participationCancellations,
            hostCancellations,
            playSessions);
    }

    private static async Task<TestSeed> SeedPlaySessionAsync(
        ApplicationDbContext dbContext,
        int maxPlayers,
        long priceVnd)
    {
        var host = CreateUser("host@test.local", "Host User");
        var guest = CreateUser("guest@test.local", "Guest User");
        dbContext.Users.AddRange(host, guest);

        var now = Clock.UtcNow;
        var post = new PlaySessionPost
        {
            CreatorUserId = host.Id,
            Title = "Evening badminton",
            Description = "Friendly doubles session",
            CourtName = "Court A",
            CourtAddress = "123 Test Street",
            StartTime = now.AddDays(1),
            EndTime = now.AddDays(1).AddHours(2),
            MaxPlayers = maxPlayers,
            CurrentPlayers = 0,
            MalePlayers = 0,
            FemalePlayers = 0,
            ShowMalePlayers = true,
            ShowFemalePlayers = true,
            Status = PostStatus.Active,
            CreatedAt = now
        };
        post.SetPricePerPlayer(priceVnd);
        dbContext.PlaySessionPosts.Add(post);

        await dbContext.SaveChangesAsync(CancellationToken.None);

        return new TestSeed(host, guest, post);
    }

    private static ApplicationUser CreateUser(string email, string fullName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            FullName = fullName,
            EmailConfirmed = true
        };
    }

    private sealed record TestSeed(
        ApplicationUser Host,
        ApplicationUser Guest,
        PlaySessionPost Post);

    private sealed record TestServices(
        IJoinRequestService JoinRequests,
        IPaymentService Payments,
        IWalletService Wallets,
        IParticipationCancellationService ParticipationCancellations,
        IHostPlaySessionCancellationService HostCancellations,
        IPlaySessionPostService PlaySessions);
}
