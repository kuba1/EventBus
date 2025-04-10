using Xunit.Internal;

namespace Jgss.EventBus.IntegrationTests;

public class BusTests
{
    private readonly Mock<ILogger<Bus>> loggerMock = new();
    private readonly Mock<ISubscriptionFactory> subscriptionFactoryMock = new();
    private readonly Mock<ISubscriptionImplementation> subscriptionMock = new();

    record SomeEvent : IEvent
    {
        public string FirstProperty { get; init; } = string.Empty;
        public int SecondProperty { get; init; }
        public bool ThirdProperty { get; init; }
        public double FourthProperty { get; init; }
    }

    record AnotherEvent : IEvent
    {
        public string FirstProperty { get; init; } = string.Empty;
        public int SecondProperty { get; init; }
        public bool ThirdProperty { get; init; }
        public double FourthProperty { get; init; }
    }

    record Event1 : IEvent {}
    record Event2 : IEvent {}
    record Event3 : IEvent {}
    record Event4 : IEvent {}
    record Event5 : IEvent {}
    record Event6 : IEvent {}

    [Fact(DisplayName = "Given subscription with synchronous and asynchronous handlers when events are published then all handlers are executed")]
    public async Task Given_subscription_with_synchronous_and_asynchronous_handlers_when_events_are_published_then_all_handlers_are_executed()
    {
        var subscriptionFactory = new SubscriptionFactory();
        var bus = new Bus(loggerMock.Object, subscriptionFactory);

        var someEventReceived = new ManualResetEventSlim();
        var event5Received = new ManualResetEventSlim();

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var cancellationToken = cancellation.Token;

        var firstSubscription = bus.Subscribe("FirstBackgroundService");
        var secondSubscription = bus.Subscribe("SecondBackgroundService");

        secondSubscription
            .Synchronously("First synchronous handler")
            .Handle((SomeEvent someEvent) => someEventReceived.Set())
            .Handle((Event1 event1) => { })
            .Handle((Event2 event2) => { });

        secondSubscription
            .Synchronously("Second synchronous handler")
            .Handle((AnotherEvent otherEvent) => { })
            .Handle((Event3 event3) => { })
            .Handle((Event4 event3) => { });

        secondSubscription
            .Asynchronously("Asynchronous handler")
            .Handle(async (Event5 event5) => 
            {
                event5Received.Set();

                await Task.CompletedTask;
            })
            .Handle(async (Event6 event6) => await Task.CompletedTask);

        var firstBackgroundService = Task.Run(async() =>
        {
            try
            {
                await firstSubscription.ProcessEventsAsync(cancellationToken);
            }
            catch
            {
            }
        },
        CancellationToken.None);

        var secondBackgroundService = Task.Run(async () =>
        {
            try
            {
                await secondSubscription.ProcessEventsAsync(cancellationToken);
            }
            catch
            {
            }
        },
        CancellationToken.None);

        firstSubscription.Publish(new SomeEvent
        {
            FirstProperty = "Some first property data",
            SecondProperty = 5,
            ThirdProperty = true,
            FourthProperty = 9.999
        });

        firstSubscription.Publish(new Event5());

        await WaitUntilSetAsync(TimeSpan.FromSeconds(30), someEventReceived, event5Received);

        cancellation.Cancel();

        await Task.WhenAll(firstBackgroundService, secondBackgroundService);

        Assert.True(someEventReceived.IsSet);
        Assert.True(event5Received.IsSet);
    }

    private async Task WaitUntilSetAsync(TimeSpan timeout, params ManualResetEventSlim[] eventsToSet)
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.CancelAfter(timeout);
        var cancellationToken = cancellationTokenSource.Token;

        await Task.WhenAll(eventsToSet.Select(e => Task.Run(() => e.Wait(cancellationToken))));
    }
}