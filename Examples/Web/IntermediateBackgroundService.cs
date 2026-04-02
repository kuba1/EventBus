using Jgss.EventBus.Examples.Web.Events;

namespace Jgss.EventBus.Examples.Web;

public sealed class IntermediateBackgroundService : BackgroundService
{
    private const string ServiceName = nameof(IntermediateBackgroundService);

    private readonly ILogger logger;
    private readonly ISubscription subscription;

    public IntermediateBackgroundService(ILogger<IntermediateBackgroundService> logger, IBus bus)
    {
        this.logger = logger;

        logger.LogInformation("Creating {ServiceName}", ServiceName);

        subscription = bus.Subscribe(nameof(IntermediateBackgroundService));

        subscription
            .Asynchronously()
            .Handle<RequestReceived>(HandleRequestReceivedAsync);
    }

    protected override async Task ExecuteAsync(CancellationToken gracefulShutdownToken)
    {
        logger.LogInformation("Starting {ServiceName}", ServiceName);

        await subscription.ProcessEventsAsync(gracefulShutdownToken);

        logger.LogInformation("Exiting {ServiceName}", ServiceName);
    }

    private Task HandleRequestReceivedAsync(RequestReceived requestReceived)
    {
        logger.LogInformation(
            "Intermediate service received a request: {Message}",
            requestReceived.Message);

        subscription.Publish(new IntermediateEvent(requestReceived) { Message = "Intermediate step completed" });

        logger.LogInformation("Intermediate event published");

        return Task.CompletedTask;
    }
}
