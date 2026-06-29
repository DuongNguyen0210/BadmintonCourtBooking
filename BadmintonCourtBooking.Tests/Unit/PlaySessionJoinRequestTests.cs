using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Tests.Unit;

public sealed class PlaySessionJoinRequestTests
{
    [Fact]
    public void Approve_ChangesPendingRequestToAwaitingPayment()
    {
        var requestedAt = new DateTimeOffset(2026, 6, 29, 10, 0, 0, TimeSpan.Zero);
        var reviewedAt = requestedAt.AddMinutes(5);
        var paymentDueAt = reviewedAt.AddMinutes(15);
        var request = PlaySessionJoinRequest.Create(Guid.NewGuid(), "guest-1", requestedAt);

        request.Approve("host-1", reviewedAt, paymentDueAt);

        Assert.Equal(JoinRequestStatus.AwaitingPayment, request.Status);
        Assert.Equal(reviewedAt, request.ReviewedAtUtc);
        Assert.Equal("host-1", request.ReviewedByHostId);
        Assert.Equal(paymentDueAt, request.PaymentDueAtUtc);
        Assert.Equal(reviewedAt, request.UpdatedAtUtc);
    }

    [Fact]
    public void MarkAsPaid_ChangesAwaitingPaymentRequestToJoined()
    {
        var now = new DateTimeOffset(2026, 6, 29, 10, 0, 0, TimeSpan.Zero);
        var request = PlaySessionJoinRequest.Create(Guid.NewGuid(), "guest-1", now);
        request.Approve("host-1", now.AddMinutes(1), now.AddMinutes(16));

        request.MarkAsPaid(now.AddMinutes(2));

        Assert.Equal(JoinRequestStatus.Joined, request.Status);
        Assert.Equal(now.AddMinutes(2), request.PaidAtUtc);
    }

    [Fact]
    public void Approve_RejectsRequestThatIsNotPending()
    {
        var now = new DateTimeOffset(2026, 6, 29, 10, 0, 0, TimeSpan.Zero);
        var request = PlaySessionJoinRequest.Create(Guid.NewGuid(), "guest-1", now);
        request.Cancel(now.AddMinutes(1));

        var exception = Assert.Throws<DomainException>(() =>
            request.Approve("host-1", now.AddMinutes(2), now.AddMinutes(17)));

        Assert.Equal("JOIN_REQUEST_NOT_PENDING", exception.Code);
    }
}
