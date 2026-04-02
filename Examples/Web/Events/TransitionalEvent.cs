using Jgss.EventBus;

namespace Jgss.EventBus.Examples.Web.Events;

public class IntermediateEvent(RequestReceived request) : IEventWithId
{
    public Guid Id { get; init; } = request.Id;

    public string Message { get; init; } = string.Empty;
}
