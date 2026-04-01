using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Jgss.EventBus;
using Jgss.EventBus.Examples.Web.Events;

namespace Jgss.EventBus.Examples.Web;

public sealed class TransitionalBackgroundService : BackgroundService
{
    private const string ServiceName = nameof(TransitionalBackgroundService);

    private readonly ILogger logger;
    private readonly ISubscription subscription;

    public TransitionalBackgroundService(ILogger<TransitionalBackgroundService> logger, IBus bus)
    {
        this.logger = logger;

        logger.LogInformation("Creating {ServiceName}", ServiceName);

        subscription = bus.Subscribe(nameof(TransitionalBackgroundService));

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
            "Transitional service received a request: {Message}",
            requestReceived.Message);

        subscription.Publish(new TransitionalEvent(requestReceived) { Message = "Transitional step completed" });

        logger.LogInformation("Transitional event published");

        return Task.CompletedTask;
    }
}
