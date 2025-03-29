namespace Jgss.EventBus;

public interface ISubscription
{
    public ISynchronousQueue HandleSynchronously(string? queueName = null);

    public IAsynchronousQueue HandleAsynchronously(string? queueName = null);
}