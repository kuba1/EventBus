namespace Jgss.EventBus.UnitTests;

public class BusTests
{
    private readonly Mock<ILogger<Bus>> loggerMock = new();
    private readonly Mock<ISubscriptionFactory> subscriptionFactoryMock = new();
    private readonly Mock<ISubscriptionImplementation> subscriptionMock = new();

    public BusTests()
    {
        subscriptionFactoryMock
            .Setup(f => f.CreateSubscription(It.IsAny<string>(), It.IsAny<IEventRouter>()))
            .Returns(subscriptionMock.Object);
    }

    [Fact(DisplayName = "Given initialized bus when it is subscribed to then it asks subscription factory for subscription instance")]
    public void Given_initialized_bus_when_it_is_subscribed_to_then_it_asks_subscription_factory_for_subscription_instance()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        bus.Subscribe("Test subscription");

        subscriptionFactoryMock.Verify(f => f.CreateSubscription("Test subscription", It.IsAny<IEventRouter>()), Times.Once);
    }

    [Fact(DisplayName = "Given subscribed subscription when it is unsubscribed then unsubscribing succeeds")]
    public void Given_subscribed_subscription_when_it_is_unsubscribed_then_unsubscribing_succeeds()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        var subscription = bus.Subscribe("Test subscription");

        bus.Unsubscribe(subscription);
    }

    [Fact(DisplayName = "Given unsubscribed subscription when it is unsubscribed again then unsubscribing succeeds")]
    public void Given_unsubscribed_subscription_when_it_is_unsubscribed_again_then_unsubscribing_succeeds()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        var subscription = bus.Subscribe("Test subscription");

        bus.Unsubscribe(subscription);
        bus.Unsubscribe(subscription);
    }

    record TestEvent : IEvent
    {
        public string TestProperty { get; init; } = "Test property value";
    }

    [Fact(DisplayName = "Given subscribed subscription when event is published then it is received by subscription")]
    public void Given_subscribed_subscription_when_event_is_published_then_it_is_received_by_subscription()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        subscriptionMock.Setup(s => s.Receive(It.IsAny<IEvent>()));

        var subscription = bus.Subscribe("Test subscription");

        var eventToPublish = new TestEvent();
        bus.Publish(eventToPublish);

        subscriptionMock.Verify(s => s.Receive(eventToPublish), Times.Once);
    }
}