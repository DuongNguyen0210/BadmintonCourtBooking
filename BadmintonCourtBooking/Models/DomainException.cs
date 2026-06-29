namespace BadmintonCourtBooking.Models;

public sealed class DomainException(string code, string message) : InvalidOperationException(message)
{
    public string Code { get; } = code;
}
