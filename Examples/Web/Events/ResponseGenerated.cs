using Jgss.EventBus;

namespace Jgss.EventBus.Examples.Web.Events;

public class ResponseGenerated(IntermediateEvent intermediateEvent) : IEventWithId
{
    public Guid Id { get; init; } = intermediateEvent.Id;

    public string Message { get; init; } = string.Empty;
}
