using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Dtos.Development;

public sealed class DevelopmentTopUpRequest
{
    [Range(1, long.MaxValue)]
    public long AmountVnd { get; set; }
}
