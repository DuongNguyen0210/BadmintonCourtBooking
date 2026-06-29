using BadmintonCourtBooking.Dtos.Payments;

namespace BadmintonCourtBooking.Services;

public interface IPaymentService
{
    Task<ServiceResult<ConfirmPaymentResponse>> ConfirmPaymentAsync(
        Guid joinRequestId,
        string userId,
        CancellationToken cancellationToken);
}
