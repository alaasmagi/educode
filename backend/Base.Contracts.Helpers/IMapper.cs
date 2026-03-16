namespace Base.Contracts.Helpers;

public interface IMapper<TUpperEntity, TLowerEntity> : IMapper<TUpperEntity, TLowerEntity, Guid> where TUpperEntity : class
                                                                                                where TLowerEntity : class
{
}

public interface IMapper<TUpperEntity, TLowerEntity, TKey> where TKey : IEquatable<TKey> where TUpperEntity : class 
                                                                                            where TLowerEntity : class
{
    public TUpperEntity? Map(TLowerEntity? entity);
    public TLowerEntity? Map(TUpperEntity? entity);
}

