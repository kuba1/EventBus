namespace Jgss.EventBus;

public interface ISynchronousQueue
{
    ISynchronousQueue Handle<TEvent>(Action<TEvent> handler) where TEvent: IEvent;
}