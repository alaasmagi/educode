namespace Base.Contracts.Domain;

public interface IBaseEntitySoftDelete
{
    public bool IsDeleted { get; set; }
}