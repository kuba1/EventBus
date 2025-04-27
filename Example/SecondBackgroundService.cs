using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Jgss.EventBus;

public class SecondBackgroundService : BackgroundService
{
    private const string ServiceName = nameof(SecondBackgroundService);

    private readonly ILogger logger;
    private readonly IBus bus;
    private readonly ISubscription subscription;

    public SecondBackgroundService(ILogger<SecondBackgroundService> logger, IBus bus)
    {
        this.logger = logger;
        this.bus = bus;

        logger.LogInformation("Creating {ServiceName}", ServiceName);

        subscription = bus.Subscribe(ServiceName);
    }

    protected override async Task ExecuteAsync(CancellationToken gracefulShutdownToken)
    {
        logger.LogInformation("Starting {ServiceName}", ServiceName);

        logger.LogInformation("Publishing {EventTypeName}", typeof(SecondBackgroundServiceStarted).Name);

        subscription.Publish(new SecondBackgroundServiceStarted
        {
            Message = $"Greetings from {ServiceName}"
        });

        await Task.Delay(1000);

        logger.LogInformation("Unsubscribing");

        bus.Unsubscribe(subscription);

        logger.LogInformation("Exiting {ServiceName}", ServiceName);
    }
}