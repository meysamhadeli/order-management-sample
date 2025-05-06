namespace BuildingBlocks.Core.Model;

public interface IEntity<T> : IEntity
{
    public T Id { get; set; }
}

public interface IEntity : IVersion
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
}
