using System.ComponentModel.DataAnnotations;
using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Dtos.Participations;

public sealed class CancelParticipationRequest
{
    [Required]
    public CancellationRefundChoice RefundChoice { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public string? WaiveRefundConfirmation { get; set; }
}
