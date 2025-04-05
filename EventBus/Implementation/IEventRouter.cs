namespace Jgss.EventBus.Implementation;

internal interface IEventRouter
{
    void Publish(IEvent publishedEvent);
}