using System.Collections.Concurrent;

namespace Jgss.EventBus.Implementation;

internal class Subscription : ISubscriptionImplementation
{
    private readonly IEventRouter eventRouter;

    private readonly ConcurrentBag<ISynchronousHandler> synchronous = new();
    private readonly ConcurrentBag<IAsynchronousHandler> asynchronous = new();

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

        return handler;
    }

    public IAsynchronousHandler Asynchronously(string? handlerName = null)
    {
        var handler = new AsynchronousHandler(handlerName);

        return handler;
    }

    public void Publish(IEvent eventToPublish) => eventRouter.Publish(eventToPublish);

    public async Task WaitForEventsAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}