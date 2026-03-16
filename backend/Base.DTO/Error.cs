using Base.Contracts.DTO;

namespace Base.DTO;

public class Error<T>(T code, string message) : IError<T> where T : IEquatable<T>
{
    public T Code { get; set; } = code;
    public string Message { get; set; } = message;
}