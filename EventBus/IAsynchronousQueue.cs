namespace Jgss.EventBus;

public interface IAsynchronousQueue
{
    IAsynchronousQueue Handle<TEvent>(Func<TEvent, Task> handler) where TEvent: IEvent;
}