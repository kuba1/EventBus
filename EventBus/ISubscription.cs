namespace Jgss.EventBus;

public interface ISubscription
{
    public Guid Id { get; }
    public string Name { get; }

    ISynchronousHandler Synchronously(string? queueName = null);
    IAsynchronousHandler Asynchronously(string? queueName = null);
    void Publish(IEvent eventToPublish);
    Task WaitForEventsAsync(CancellationToken cancellationToken);
}