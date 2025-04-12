using Jgss.EventBus;

internal interface IEventProcessor
{
    Task ProcessEventsAsync(CancellationToken cancellationToken);
    void Receive(IEvent receivedEvent);
}