namespace Common.Domain;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}

public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;
}
