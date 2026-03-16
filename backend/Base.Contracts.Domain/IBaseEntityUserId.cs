namespace Base.Contracts.Domain;

public interface IBaseEntityUserId : IBaseEntityUserId<Guid>
{
}

public interface IBaseEntityUserId<TKey> where TKey : IEquatable<TKey>
{
    public TKey UserId { get; set; }
}