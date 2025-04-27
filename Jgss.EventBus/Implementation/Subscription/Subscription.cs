using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Jgss.EventBus.Implementation;

internal class Subscription : EventProcessor, ISubscriptionImplementation
{
    private readonly ILogger<Subscription> logger;
    private readonly IEventRouter eventRouter;
    private readonly ConcurrentBag<IEventProcessor> handlers = [];

    public Guid Id { get; init; }
    public string Name { get; init; }

    internal Subscription(ILogger<Subscription> logger, string? name, IEventRouter eventRouter)
    {
        this.logger = logger;

        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name))
            name = $"Subscription|{Id}";

        Name = name;

        this.eventRouter = eventRouter;
    }

    public ISynchronousHandler Synchronously(string? handlerName = null)
    {
        var handler = new SynchronousHandler(handlerName);

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Adding {HandlerName} synchronous handler", handlerName);

        handlers.Add(handler);

        return handler;
    }

    public IAsynchronousHandler Asynchronously(string? handlerName = null)
    {
        var handler = new AsynchronousHandler(handlerName);

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Adding {HandlerName} asynchronous handler", handlerName);

        handlers.Add(handler);

        return handler;
    }

    public void Publish(IEvent eventToPublish) => eventRouter.Publish(eventToPublish);

    public async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        var handlersTasks = handlers
            .Select(h => h.ProcessEventsAsync(cancellationToken))
            .ToList();

        await StartAsync(cancellationToken);

        await Task.WhenAll(handlersTasks);
    }

    public void Receive(IEvent receivedEvent) => Process(receivedEvent);

    protected override void Dispatch(IEvent receivedEvent)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Dispatching {EventType} event", receivedEvent.GetType().Name);

        foreach (var handler in handlers)
            handler.Receive(receivedEvent);
    }
}