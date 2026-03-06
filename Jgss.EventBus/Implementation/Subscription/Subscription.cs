using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Jgss.EventBus.Implementation;

internal sealed class Subscription : ISubscriptionImplementation
{
    private readonly ILogger<Subscription> logger;
    private readonly IEventPublisher eventPublisher;
    private readonly EventProcessingTask eventProcessor = new();
    private readonly ConcurrentBag<IEventProcessor> handlers = [];

    public Guid Id { get; init; }
    public string Name { get; init; }

    internal Subscription(ILogger<Subscription> logger, string? name, IEventPublisher eventPublisher)
    {
        this.logger = logger;

        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name))
            name = $"Subscription|{Id}";

        Name = name;

        this.eventPublisher = eventPublisher;

        eventProcessor.EventDispatched += Dispatch;
    }

    public ISynchronousHandler Synchronously(string? handlerName = null)
    {
        var handler = new SynchronousHandler(handlerName);

        logger.LogDebug("[{Name}] Adding {HandlerName} synchronous handler", Name, handlerName);

        handlers.Add(handler);

        return handler;
    }

    public IAsynchronousHandler Asynchronously(string? handlerName = null)
    {
        var handler = new AsynchronousHandler(handlerName);

        logger.LogDebug("[{Name}] Adding {HandlerName} asynchronous handler", Name, handlerName);

        handlers.Add(handler);

        return handler;
    }

    public void Publish(IEvent eventToPublish)
    {
        logger.LogDebug("[{Name}] Publishing {EventTypeName} event", Name, eventToPublish.GetType().Name);

        eventPublisher.Publish(eventToPublish);
    }

    public async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        var handlersTasks = handlers
            .Select(h => h.ProcessEventsAsync(cancellationToken))
            .ToList();

        await eventProcessor.ProcessEventsAsync(cancellationToken);

        await Task.WhenAll(handlersTasks);
    }

    public void Receive(IEvent receivedEvent)
    {
        logger.LogDebug("[{Name} Receiving {EventType}", Name, receivedEvent.GetType().Name);

        eventProcessor.Receive(receivedEvent);
    }

    private void Dispatch(IEvent receivedEvent)
    {
        foreach (var handler in handlers)
            handler.Receive(receivedEvent);
    }
}