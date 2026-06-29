namespace BadmintonCourtBooking.Models;

public enum JoinRequestStatus
{
    PendingHostApproval,
    AwaitingPayment,
    Joined,
    Rejected,
    Cancelled,
    Expired
}
