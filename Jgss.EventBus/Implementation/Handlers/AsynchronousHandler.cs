namespace Jgss.EventBus.Implementation;

internal sealed class AsynchronousHandler : IAsynchronousHandler, IEventProcessor
{
    private readonly string name;
    private readonly Dictionary<Type, Action<IEvent>> handlers = [];
    private readonly EventProcessingTask eventProcessor = new();

    public AsynchronousHandler(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = $"Asynchronous handler[{Guid.NewGuid()}]";

        this.name = name;

        eventProcessor.EventDispatched += Dispatch;
    }

    public IAsynchronousHandler Handle<TEvent>(Func<TEvent, Task> handler) where TEvent: IEvent
    {
        handlers[typeof(TEvent)] = eventToHandle =>
        {
            if (eventToHandle is not TEvent actualEventToHandle)
                return;

            Task.Run(async () =>
            {
                try
                {
                    await handler(actualEventToHandle);
                }
                catch
                {
                    // Event handler threw unhandled exception
                }
            },
            CancellationToken.None);
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