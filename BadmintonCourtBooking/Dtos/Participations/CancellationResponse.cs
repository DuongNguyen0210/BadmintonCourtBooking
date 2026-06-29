using BadmintonCourtBooking.Dtos.Wallet;

namespace BadmintonCourtBooking.Dtos.Participations;

public sealed record CancellationResponse(
    Guid CancellationId,
    Guid ParticipantId,
    long OriginalAmountVnd,
    long RefundAmountVnd,
    long CancellationFeeVnd,
    string RefundChoice,
    WalletResponse Wallet);
