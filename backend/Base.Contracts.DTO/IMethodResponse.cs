namespace Base.Contracts.DTO;

public interface IMethodResponse<TValue> : IMethodResponse<TValue, IError> where TValue : class {}
public interface IMethodResponse<TValue, TError> where TValue : class where TError : IError
{
    public bool Successful { get; }
    public TValue? Value { get; }
    public TError? Error { get; }
}