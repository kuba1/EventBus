using Jgss.EventBus;

namespace Jgss.EventBus.Examples.Web.Events;

public class ResponseGenerated(TransitionalEvent transitionalEvent) : IEventWithId
{
    public Guid Id { get; init; } = transitionalEvent.Id;

    public string Message { get; init; } = string.Empty;
}
