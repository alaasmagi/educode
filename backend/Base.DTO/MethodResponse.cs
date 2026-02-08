using Base.Domain;

namespace Base.DTO;

public class MethodResponse<T>
{
    public bool Successful { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private MethodResponse(T value)
    {
        Successful = true;
        Value = value;
    }

    private MethodResponse(Error error)
    {
        Successful = false;
        Error = error;
    }

    public static MethodResponse<T> Success(T value) => new(value);
    public static MethodResponse<T> Failure(Error error) => new(error);
}