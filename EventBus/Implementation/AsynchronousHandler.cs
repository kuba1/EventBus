namespace Jgss.EventBus.Implementation;

internal class AsynchronousHandler : IAsynchronousHandler
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
}