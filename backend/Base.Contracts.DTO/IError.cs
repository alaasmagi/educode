namespace Base.Contracts.DTO;

public interface IError : IError<string>
{
}

public interface IError<T> where T : IEquatable<T>
{
    public T Code { get; set; }
    public string Message { get; set; }
}