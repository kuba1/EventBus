namespace Jgss.EventBus.Implementation;

internal interface IEventPublisher
{
    void Publish(IEvent publishedEvent);
}