using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using BuildingBlocks.EventBus.Abstractions;
using BuildingBlocks.EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BuildingBlocks.EventBus.RabbitMQ;

public class RabbitMQEventBus : IEventBus, IAsyncDisposable
{
    private const string ExchangeName = "enterprise_event_bus";
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, List<Type>> _handlers = new();
    private IChannel? _channel;

    public RabbitMQEventBus(
        IConnection connection,
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel is null)
        {
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
        }
        return _channel;
    }

    public async Task PublishAsync(IntegrationEvent eventData)
    {
        var channel = await GetChannelAsync();
        var eventName = eventData.GetType().Name;
        var message = JsonSerializer.Serialize(eventData, eventData.GetType());
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: eventName,
            body: body);

        _logger.LogInformation("Published event: {EventName} - {EventId}", eventName, eventData.Id);
    }

    public void Subscribe<TIntegrationEvent, TIntegrationEventHandler>()
        where TIntegrationEvent : IntegrationEvent
        where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
    {
        var eventName = typeof(TIntegrationEvent).Name;

        _handlers.GetOrAdd(eventName, _ => new List<Type>())
            .Add(typeof(TIntegrationEventHandler));

        _ = SubscribeInternalAsync(eventName);
    }

    private async Task SubscribeInternalAsync(string eventName)
    {
        var channel = await GetChannelAsync();
        var queueName = eventName;

        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(queueName, ExchangeName, eventName);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, args) =>
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            await ProcessEventAsync(eventName, message);
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);
        _logger.LogInformation("Subscribed to event: {EventName}", eventName);
    }

    public void Unsubscribe<TIntegrationEvent, TIntegrationEventHandler>()
        where TIntegrationEvent : IntegrationEvent
        where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
    {
        if (_handlers.TryGetValue(typeof(TIntegrationEvent).Name, out var handlers))
        {
            handlers.Remove(typeof(TIntegrationEventHandler));
        }
    }

    private async Task ProcessEventAsync(string eventName, string message)
    {
        if (!_handlers.TryGetValue(eventName, out var handlerTypes))
            return;

        using var scope = _serviceProvider.CreateScope();

        foreach (var handlerType in handlerTypes)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is null) continue;

            var eventType = handlerType.GetInterfaces()
                .First(i => i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                .GetGenericArguments()[0];

            var integrationEvent = JsonSerializer.Deserialize(message, eventType);
            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            var handleMethod = concreteType.GetMethod("Handle");

            if (handleMethod is not null && integrationEvent is not null)
            {
                var task = (Task)handleMethod.Invoke(handler, new object[] { integrationEvent })!;
                await task;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        _connection?.Dispose();
    }
}
