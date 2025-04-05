namespace Jgss.EventBus;

public interface ISynchronousHandler
{
    ISynchronousHandler Handle<TEvent>(Action<TEvent> handler) where TEvent: IEvent;
}