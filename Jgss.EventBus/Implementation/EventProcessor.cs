using System.Collections.Concurrent;

namespace Jgss.EventBus;

/// <summary>
/// Dispatches events one-by-one to handling code on a separate task.
/// The idea is for events to be consumed as quickly as possible from the point of view of code
/// calling Process method.
/// </summary>
internal abstract class EventProcessor
{
    private readonly BlockingCollection<IEvent> events = [];

    /// <summary>
    /// Start event processing task, it's going to run until canceled
    /// </summary>
    protected async Task StartAsync(CancellationToken cancellationToken)
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
    protected void Process(IEvent processedEvent) => events.Add(processedEvent);

    /// <summary>
    /// Dispatch an event to handling code
    /// </summary>
    protected abstract void Dispatch(IEvent processedEvent);

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
            Dispatch(processedEvent);
    }
}