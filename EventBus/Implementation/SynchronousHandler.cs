namespace Jgss.EventBus.Implementation;

internal class SynchronousHandler : ISynchronousHandler
{
    private readonly string name;
    private readonly Dictionary<Type, Action<IEvent>> handlers = new();

    public SynchronousHandler(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = $"Synchronous handler[{Guid.NewGuid().ToString()}]";

        this.name = name;
    }

    public ISynchronousHandler Handle<TEvent>(Action<TEvent> handler) where TEvent: IEvent
    {
        handlers[typeof(TEvent)] = ((IEvent eventToHandle) =>
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
        });

        return this;
    }
}