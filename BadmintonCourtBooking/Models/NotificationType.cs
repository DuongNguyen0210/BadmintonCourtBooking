namespace BadmintonCourtBooking.Models;

public enum NotificationType
{
    JoinRequested,
    JoinApproved,
    JoinRejected,
    PaymentRequired,
    PaymentCompleted,
    ParticipantJoined,
    JoinRequestExpired,
    ParticipantCancelled,
    RefundCompleted,
    NoRefundCancellation,
    HostCancelledSession,
    ManualCompensationReceived
}
