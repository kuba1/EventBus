using System.Collections.Concurrent;

namespace Jgss.EventBus;

/// <summary>
/// Dispatches events one-by-one to handling code on a separate task.
/// The idea is for events to be consumed as quickly as possible from the point of view of code
/// calling Process method.
/// </summary>
internal sealed class EventProcessingTask : IEventProcessor
{
    private readonly BlockingCollection<IEvent> events = [];

    public event Action<IEvent>? EventDispatched;

    /// <summary>
    /// Start event processing task, it's going to run until canceled
    /// </summary>
    public async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        await Task
            .Factory
            .StartNew(() => UseCollectionAndProcessEvents(cancellationToken), TaskCreationOptions.LongRunning)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Add an event to collection for processing
    /// </summary>
    /// <param name="processedEvent"></param>
    public void Receive(IEvent processedEvent) => events.Add(processedEvent);

    /// <summary>
    /// Make sure the collection is disposed when canceled and start consuming events
    /// </summary>
    private void UseCollectionAndProcessEvents(CancellationToken cancellationToken)
    {
        using (events)
        {
            try
            {
                ConsumeEvents(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }
    }

    /// <summary>
    /// Process events until canceled
    /// </summary>
    private void ConsumeEvents(CancellationToken cancellationToken)
    {
        foreach (var processedEvent in events.GetConsumingEnumerable(cancellationToken))
            EventDispatched?.Invoke(processedEvent);
    }
}