namespace Jgss.EventBus;

public interface IAsynchronousHandler
{
    IAsynchronousHandler Handle<TEvent>(Func<TEvent, Task> handler) where TEvent: IEvent;
}