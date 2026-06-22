namespace BuildingBlocks.EventBus.Events;

public abstract class IntegrationEvent
{
    public Guid Id { get; private init; }
    public DateTime CreationDate { get; private init; }

    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    protected IntegrationEvent(Guid id, DateTime creationDate)
    {
        Id = id;
        CreationDate = creationDate;
    }
}
