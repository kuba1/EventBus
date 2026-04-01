using Jgss.EventBus;

namespace Jgss.EventBus.Examples.Web.Events;

public class ResponseGenerated : IEventWithId
{
    public Guid Id { get; init; }

    public string Message { get; init; } = string.Empty;

    public ResponseGenerated(RequestReceived request) => Id = request.Id;
}
