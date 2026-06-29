namespace BadmintonCourtBooking.Services;

public sealed class ServiceResult<T>
{
    private ServiceResult(T value)
    {
        Value = value;
    }

    private ServiceResult(ServiceError error)
    {
        Error = error;
    }

    public T? Value { get; }

    public ServiceError? Error { get; }

    public bool Succeeded => Error is null;

    public static ServiceResult<T> Success(T value) => new(value);

    public static ServiceResult<T> Failure(string code, string message) => new(new ServiceError(code, message));
}
