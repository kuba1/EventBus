namespace Jgss.EventBus.Implementation;

internal sealed class SynchronousHandler : ISynchronousHandler, IEventProcessor
{
    private readonly string name;
    private readonly Dictionary<Type, Action<IEvent>> handlers = new();
    private readonly EventProcessingTask eventProcessor = new();

    public SynchronousHandler(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = $"Synchronous handler[{Guid.NewGuid()}]";

        this.name = name;

        eventProcessor.EventDispatched += Dispatch;
    }

    public ISynchronousHandler Handle<TEvent>(Action<TEvent> handler) where TEvent: IEvent
    {
        handlers[typeof(TEvent)] = eventToHandle =>
        {
            if (eventToHandle is not TEvent actualEventToHandle)
                return;

            try
            {
                handler(actualEventToHandle);
            }
            catch
            {
                // Event handler threw unhandled exception
            }
        };

        return this;
    }

    public async Task ProcessEventsAsync(CancellationToken cancellationToken) =>
        await eventProcessor.ProcessEventsAsync(cancellationToken);

    public void Receive(IEvent receivedEvent) => eventProcessor.Receive(receivedEvent);

    private void Dispatch(IEvent processedEvent)
    {
        if (handlers.TryGetValue(processedEvent.GetType(), out var handler))
            handler.Invoke(processedEvent);
    }
}