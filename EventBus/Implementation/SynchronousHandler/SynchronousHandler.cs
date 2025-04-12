using System.Collections.Concurrent;

namespace Jgss.EventBus.Implementation;

internal class SynchronousHandler : EventProcessor, ISynchronousHandlerImplementation
{
    private readonly string name;
    private readonly Dictionary<Type, Action<IEvent>> handlers = new();
    private readonly BlockingCollection<IEvent> events = [];

    public SynchronousHandler(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = $"Synchronous handler[{Guid.NewGuid()}]";

        this.name = name;
    }

    public ISynchronousHandler Handle<TEvent>(Action<TEvent> handler) where TEvent: IEvent
    {
        handlers[typeof(TEvent)] = eventToHandle =>
        {
            if (eventToHandle is TEvent actualEventToHandle)
            {
                try
                {
                    handler(actualEventToHandle);
                }
                catch
                {
                }
            }
        };

        return this;
    }

    public async Task ProcessEventsAsync(CancellationToken cancellationToken) => await StartAsync(cancellationToken);

    public void Receive(IEvent receivedEvent) => Process(receivedEvent);

    protected override void Dispatch(IEvent processedEvent)
    {
        if (handlers.TryGetValue(processedEvent.GetType(), out var handler))
            handler.Invoke(processedEvent);
    }
}