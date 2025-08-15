namespace Jgss.EventBus.UnitTests;

public class BusTests
{
    private readonly Mock<ILogger<Bus>> loggerMock = new();
    private readonly Mock<ISubscriptionFactory> subscriptionFactoryMock = new();
    private readonly Mock<ISubscriptionImplementation> firstSubscriptionMock = new();
    private readonly Mock<ISubscriptionImplementation> secondSubscriptionMock = new();
    private readonly Mock<ISubscriptionImplementation> thirdSubscriptionMock = new();

    public BusTests()
    {
        firstSubscriptionMock.Setup(s => s.Receive(It.IsAny<IEvent>()));
        firstSubscriptionMock.SetupGet(s => s.Name).Returns("Some subscription");
        firstSubscriptionMock.SetupGet(s => s.Id).Returns(Guid.NewGuid());

        subscriptionFactoryMock
            .Setup(f => f.CreateSubscription("Some subscription", It.IsAny<IEventRouter>()))
            .Returns(firstSubscriptionMock.Object);

        secondSubscriptionMock.Setup(s => s.Receive(It.IsAny<IEvent>()));
        secondSubscriptionMock.SetupGet(s => s.Name).Returns("Some other subscription");
        secondSubscriptionMock.SetupGet(s => s.Id).Returns(Guid.NewGuid());

        subscriptionFactoryMock
            .Setup(f => f.CreateSubscription("Some other subscription", It.IsAny<IEventRouter>()))
            .Returns(secondSubscriptionMock.Object);

        thirdSubscriptionMock.Setup(s => s.Receive(It.IsAny<IEvent>()));
        thirdSubscriptionMock.SetupGet(s => s.Name).Returns("Yet some other subscription");
        thirdSubscriptionMock.SetupGet(s => s.Id).Returns(Guid.NewGuid());

        subscriptionFactoryMock
            .Setup(f => f.CreateSubscription("Yet some other subscription", It.IsAny<IEventRouter>()))
            .Returns(thirdSubscriptionMock.Object);
    }

    [Fact(DisplayName = "Given initialized bus when it is subscribed to then it asks subscription factory for subscription instance")]
    public void Given_initialized_bus_when_it_is_subscribed_to_then_it_asks_subscription_factory_for_subscription_instance()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        bus.Subscribe("Some subscription");

        subscriptionFactoryMock.Verify(f => f.CreateSubscription("Some subscription", It.IsAny<IEventRouter>()), Times.Once);
    }

    [Fact(DisplayName = "Given subscribed subscription when it is unsubscribed then unsubscribing succeeds")]
    public void Given_subscribed_subscription_when_it_is_unsubscribed_then_unsubscribing_succeeds()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        var subscription = bus.Subscribe("Some subscription");

        bus.Unsubscribe(subscription);
    }

    [Fact(DisplayName = "Given unsubscribed subscription when it is unsubscribed again then unsubscribing succeeds")]
    public void Given_unsubscribed_subscription_when_it_is_unsubscribed_again_then_unsubscribing_succeeds()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        var subscription = bus.Subscribe("Some subscription");

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

        bus.Subscribe("Some subscription");

        var eventToPublish = new TestEvent();
        bus.Publish(eventToPublish);

        firstSubscriptionMock.Verify(s => s.Receive(eventToPublish), Times.Once);
    }

    [TargetSubscriptions("Some subscription")]
    class EventTargetingSomeSubscription : IEvent { }

    [Fact(DisplayName = "Given subscribed subscription and event that does not target this subscription when this event is published then it is not received by subscription")]
    public void Given_subscribed_subscription_and_event_that_does_not_target_this_subscription_when_this_event_is_published_then_it_is_not_received_by_subscription()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        bus.Subscribe("Some other subscription");

        var eventToPublish = new EventTargetingSomeSubscription();
        bus.Publish(eventToPublish);

        firstSubscriptionMock.Verify(s => s.Receive(eventToPublish), Times.Never);
    }

    [Fact(DisplayName = "Given subscribed subscription and event that targets this subscription when this event is published then it is received by subscription")]
    public void Given_subscribed_subscription_and_event_that_targets_this_subscription_when_this_event_is_published_then_it_is_received_by_subscription()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        bus.Subscribe("Some subscription");

        var eventToPublish = new EventTargetingSomeSubscription();
        bus.Publish(eventToPublish);

        firstSubscriptionMock.Verify(s => s.Receive(eventToPublish), Times.Once);
    }

    [TargetSubscriptions("Some subscription", "Some other subscription")]
    class EventTargetingTwoSubscriptions : IEvent { }

    [Fact(DisplayName = "Given an event that targets multiple subscriptions when this event is published then it is received only by targeted subscriptions")]
    public void Given_an_event_that_targets_multiple_subscriptions_when_this_event_is_published_then_it_is_received_only_by_targeted_subscriptions()
    {
        var bus = new Bus(loggerMock.Object, subscriptionFactoryMock.Object);

        bus.Subscribe("Some subscription");
        bus.Subscribe("Some other subscription");
        bus.Subscribe("Yet some other subscription");

        var eventToPublish = new EventTargetingTwoSubscriptions();
        bus.Publish(eventToPublish);

        firstSubscriptionMock.Verify(s => s.Receive(eventToPublish), Times.Once);
        secondSubscriptionMock.Verify(s => s.Receive(eventToPublish), Times.Once);
        thirdSubscriptionMock.Verify(s => s.Receive(eventToPublish), Times.Never);
    }
}