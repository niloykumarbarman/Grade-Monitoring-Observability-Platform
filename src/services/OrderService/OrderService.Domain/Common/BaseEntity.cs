namespace OrderService.Domain.Common;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}

public abstract class BaseEntity<TId> : IAuditableEntity
{
    public TId Id { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
