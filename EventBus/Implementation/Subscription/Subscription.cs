using System.Collections.Concurrent;

namespace Jgss.EventBus.Implementation;

internal class Subscription : EventProcessor, ISubscriptionImplementation
{
    private readonly IEventRouter eventRouter;

    private readonly ConcurrentQueue<IEventProcessor> handlers = [];
    private readonly BlockingCollection<IEvent> events = [];

    public Guid Id { get; init; }
    public string Name { get; init; }

    internal Subscription(string? name, IEventRouter eventRouter)
    {
        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name))
            name = $"Subscription|{Id}";

        Name = name;

        this.eventRouter = eventRouter;
    }

    public ISynchronousHandler Synchronously(string? handlerName = null)
    {
        var handler = new SynchronousHandler(handlerName);

        handlers.Enqueue(handler);

        return handler;
    }

    public IAsynchronousHandler Asynchronously(string? handlerName = null)
    {
        var handler = new AsynchronousHandler(handlerName);

        handlers.Enqueue(handler);

        return handler;
    }

    public void Publish(IEvent eventToPublish) => eventRouter.Publish(eventToPublish);

    public async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        var handlersTasks = handlers.Select(h => h.ProcessEventsAsync(cancellationToken));

        await StartAsync(cancellationToken);

        await Task.WhenAll(handlersTasks);
    }

    public void Receive(IEvent receivedEvent) => events.Add(receivedEvent);

    protected override void Dispatch(IEvent receivedEvent)
    {
        foreach (var handler in handlers)
            handler.Receive(receivedEvent);
    }
}