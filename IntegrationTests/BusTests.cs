using System.Threading;

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

    [Fact(DisplayName = "Given ... when ... then ...")]
    public async Task Given_When_Then()
    {
        var subscriptionFactory = new SubscriptionFactory();
        var bus = new Bus(loggerMock.Object, subscriptionFactory);

        var someEventReceived = new ManualResetEventSlim();

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromSeconds(30));
        var cancellationToken = cancellation.Token;

        var firstSubscription = bus.Subscribe("FirstBackgroundService");
        var secondSubscription = bus.Subscribe("SecondBackgroundService");

        secondSubscription
            .Synchronously()
            .Handle((SomeEvent someEvent) => someEventReceived.Set())
            .Handle((Event1 event1) => { })
            .Handle((Event2 event2) => { });

        secondSubscription
            .Synchronously()
            .Handle((AnotherEvent otherEvent) => { })
            .Handle((Event3 event3) => { })
            .Handle((Event4 event3) => { });

        secondSubscription
            .Asynchronously()
            .Handle(async (Event5 event5) => await Task.CompletedTask)
            .Handle(async (Event6 event6) => await Task.CompletedTask);

        var firstBackgroundService = Task.Run(async() =>
        {
            firstSubscription.Publish(new SomeEvent
            {
                FirstProperty = "Some first property data",
                SecondProperty = 5,
                ThirdProperty = true,
                FourthProperty = 9.999
            });

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

        cancellation.Cancel();

        await Task.WhenAll(firstBackgroundService, secondBackgroundService);

        Assert.True(someEventReceived.IsSet);
    }
}