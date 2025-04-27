namespace Jgss.EventBus.IntegrationTests;

public class BusTests
{
    private readonly Mock<ILogger<Bus>> loggerMock = new();

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
        var loggerFactory = new Mock<ILoggerFactory>();

        loggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());

        var subscriptionFactory = new SubscriptionFactory(loggerFactory.Object);
        var bus = new Bus(loggerMock.Object, subscriptionFactory);

        var eventsReceived = new List<(ManualResetEventSlim, string)>
        {
            (new(), "SomeEvent has not been received"),
            (new(), "Event1 has not been received"),
            (new(), "Event2 has not been received"),
            (new(), "AnotherEvent has not been received"),
            (new(), "Event3 has not been received"),
            (new(), "Event4 has not been received"),
            (new(), "Event5 has not been received"),
            (new(), "Event6 has not been received")
        };

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var cancellationToken = cancellation.Token;

        var firstSubscription = bus.Subscribe("FirstBackgroundService");
        var secondSubscription = bus.Subscribe("SecondBackgroundService");

        secondSubscription
            .Synchronously("First synchronous handler")
            .Handle((SomeEvent someEvent) => eventsReceived[0].Item1.Set())
            .Handle((Event1 event1) => eventsReceived[1].Item1.Set())
            .Handle((Event2 event2) => eventsReceived[2].Item1.Set());

        secondSubscription
            .Synchronously("Second synchronous handler")
            .Handle((AnotherEvent anotherEvent) => eventsReceived[3].Item1.Set())
            .Handle((Event3 event3) => eventsReceived[4].Item1.Set())
            .Handle((Event4 event4) => eventsReceived[5].Item1.Set());

        secondSubscription
            .Asynchronously("Asynchronous handler")
            .Handle(async (Event5 event5) => 
            {
                eventsReceived[6].Item1.Set();

                await Task.CompletedTask;
            })
            .Handle(async (Event6 event6) =>
            {
                eventsReceived[7].Item1.Set();

                await Task.CompletedTask;
            });

        var firstBackgroundService = Task.Run(async() =>
        {
            try
            {
                await firstSubscription.ProcessEventsAsync(cancellationToken);
            }
            catch (OperationCanceledException)
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
            catch (OperationCanceledException)
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
        firstSubscription.Publish(new Event3());
        firstSubscription.Publish(new Event4());
        firstSubscription.Publish(new Event1());
        firstSubscription.Publish(new Event2());
        firstSubscription.Publish(new AnotherEvent());
        firstSubscription.Publish(new Event6());

        await WaitUntilSetAsync(TimeSpan.FromSeconds(30), eventsReceived.Select(e => e.Item1).ToArray());

        cancellation.Cancel();

        await Task.WhenAll(firstBackgroundService, secondBackgroundService);

        eventsReceived.ForEach(e => 
        {
            var (eventReceived, errorMessage) = e;

            Assert.True(eventReceived.IsSet, errorMessage);
        });
    }

    private async Task WaitUntilSetAsync(TimeSpan timeout, params ManualResetEventSlim[] eventsToSet)
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.CancelAfter(timeout);
        var cancellationToken = cancellationTokenSource.Token;

        await Task.WhenAll(eventsToSet.Select(e => Task.Run(() => e.Wait(cancellationToken))));
    }
}