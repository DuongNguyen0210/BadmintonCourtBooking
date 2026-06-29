namespace BadmintonCourtBooking.Models;

public enum WalletTransactionType
{
    TopUp,
    EscrowHold,
    EscrowReleaseToHost,
    Refund,
    CancellationFee,
    FullRefund,
    ManualCompensation,
    Reversal
}
