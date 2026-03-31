using Jgss.EventBus;

public class ResponseGenerated : IEvent
{
    public string Message { get; init; } = string.Empty;
}
