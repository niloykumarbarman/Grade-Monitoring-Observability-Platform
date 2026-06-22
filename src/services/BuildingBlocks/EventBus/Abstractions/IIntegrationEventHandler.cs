using BuildingBlocks.EventBus.Events;

namespace BuildingBlocks.EventBus.Abstractions;

public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent eventData);
}

public interface IIntegrationEventHandler
{
}
