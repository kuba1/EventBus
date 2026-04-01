using Jgss.EventBus;

namespace Jgss.EventBus.Examples.Web.Events;

public class RequestReceived : IEventWithId
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Message { get; init; } = string.Empty;
}
