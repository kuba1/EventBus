namespace Jgss.EventBus.Implementation;

internal class AsynchronousHandler : EventProcessor, IAsynchronousHandlerImplementation
{
    private readonly string name;
    private readonly Dictionary<Type, Action<IEvent>> handlers = new();

    public AsynchronousHandler(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = $"Asynchronous handler[{Guid.NewGuid().ToString()}]";

        this.name = name;
    }

    public IAsynchronousHandler Handle<TEvent>(Func<TEvent, Task> handler) where TEvent: IEvent
    {
        handlers[typeof(TEvent)] = ((IEvent eventToHandle) =>
        {
            if (eventToHandle is TEvent actualEventToHandle)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await handler(actualEventToHandle);
                    }
                    catch
                    {
                    }
                },
                CancellationToken.None);
            }
        });

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