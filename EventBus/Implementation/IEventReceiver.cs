using Jgss.EventBus;

internal interface IEventReceiver
{
    void Receive(IEvent receivedEvent)
    {

    }
}