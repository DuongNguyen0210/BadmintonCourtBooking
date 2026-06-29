using BadmintonCourtBooking.Dtos.JoinRequests;
using BadmintonCourtBooking.Dtos.Wallet;

namespace BadmintonCourtBooking.Dtos.Payments;

public sealed record ConfirmPaymentResponse(
    Guid ParticipantId,
    JoinRequestResponse JoinRequest,
    WalletResponse Wallet);
