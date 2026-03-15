using System.Collections.Concurrent;

namespace Jgss.EventBus.IntegrationTests;

public sealed class BusTests : IDisposable
{
    private readonly CancellationTokenSource cancellation;

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

    public BusTests()
    {
        cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
    }

    [Fact(
        Timeout = Timeouts.Test,
        DisplayName = "Given subscription with synchronous and asynchronous handlers when events are published then all handlers are executed")]
    public async Task Given_subscription_with_synchronous_and_asynchronous_handlers_when_events_are_published_then_all_handlers_are_executed()
    {
        var loggerFactory = new Mock<ILoggerFactory>();

        loggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());

        using var bus = new Bus(loggerMock.Object, new SubscriptionFactory(loggerFactory.Object));

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
                await firstSubscription.ProcessEventsAsync(cancellation.Token);
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
                await secondSubscription.ProcessEventsAsync(cancellation.Token);
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

        await Utilities.WaitUntilSetAsync(cancellation.Token, eventsReceived.Select(e => e.Item1).ToArray());

        eventsReceived.ForEach(e => 
        {
            var (eventReceived, errorMessage) = e;

            Assert.True(eventReceived.IsSet, errorMessage);
        });
    }

    [Fact(
        Timeout = Timeouts.Test,
        DisplayName = "Given events are published in a specific order when they are received by a subscription then the order is always preserved")]
    public async Task Given_events_are_published_in_a_specific_order_when_they_are_received_by_a_subscription_then_the_order_is_always_preserved()
    {
        const int NumberOfPublishedEvents = 1000;
        const int NumberOfSubscriptions = 100;

        var eventsToPublish = new List<IEvent>();

        var random = new Random();

        // Publish a lot of events in random order
        for (var i = 0; i < NumberOfPublishedEvents; i++)
        {
            // We have 8 event types
            eventsToPublish.Add(random.NextInt64(8) switch
            {
                0 => new SomeEvent(),
                1 => new AnotherEvent(),
                2 => new Event1(),
                3 => new Event2(),
                4 => new Event3(),
                5 => new Event4(),
                6 => new Event5(),
                7 => new Event6(),
                _ => throw new InvalidOperationException()
            });
        }

        var loggerFactory = new Mock<ILoggerFactory>();

        loggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());

        using var bus = new Bus(loggerMock.Object, new SubscriptionFactory(loggerFactory.Object));

        var subscriptionsToEvents = new ConcurrentDictionary<ISubscription, List<IEvent>>();

        for (int i = 0; i < NumberOfSubscriptions; i++)
            subscriptionsToEvents[bus.Subscribe($"Subscription{i}")] = [];

        // Add multiple subscriptions, each in its own task
        foreach (var subscriptionToEvents in subscriptionsToEvents)
        {
            _ = Task.Run(async () =>
            {
                var (subscription, events) = subscriptionToEvents;

                subscription
                    .Synchronously()
                    .Handle<SomeEvent>(e => events.Add(e))
                    .Handle<AnotherEvent>(e => events.Add(e))
                    .Handle<Event1>(e => events.Add(e))
                    .Handle<Event2>(e => events.Add(e))
                    .Handle<Event3>(e => events.Add(e))
                    .Handle<Event4>(e => events.Add(e))
                    .Handle<Event5>(e => events.Add(e))
                    .Handle<Event6>(e => events.Add(e));

                await subscription.ProcessEventsAsync(cancellation.Token);
            },
            CancellationToken.None);
        }

        // Publish all events
        foreach (var eventToPublish in eventsToPublish)
            bus.Publish(eventToPublish);

        var allEventsReceived = new ManualResetEventSlim();

        SignalWhenAllEventsReceived(allEventsReceived, subscriptionsToEvents, NumberOfPublishedEvents, cancellation.Token);

        await Utilities.WaitUntilSetAsync(cancellation.Token, allEventsReceived);

        Assert.Equal(NumberOfSubscriptions, subscriptionsToEvents.Values.Count);

        foreach (var receivedEventsForSubscription in subscriptionsToEvents.Values)
            Assert.Equal(eventsToPublish, receivedEventsForSubscription);
    }

    [Fact(
        Timeout = Timeouts.Test,
        DisplayName = "Given multiple asynchronous subscriptions when some publish response events then event order is preserved")]
    public async Task Given_multiple_asynchronous_subscriptions_when_some_publish_response_events_then_event_order_is_preserved()
    {
        const int NumberOfSubscriptions = 100;

        var loggerFactory = new Mock<ILoggerFactory>();

        loggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());

        using var bus = new Bus(loggerMock.Object, new SubscriptionFactory(loggerFactory.Object));

        var firstSubscription = bus.Subscribe();
        var secondSubscription = bus.Subscribe();

        var otherSubscriptions = new List<ISubscription>();

        for (int i = 0; i < NumberOfSubscriptions; i++)
            otherSubscriptions.Add(bus.Subscribe());

        // Publish from first subscription, second one should respond to
        // Event3 with SomeEvent
        var eventsToPublishFirst = new List<IEvent>
        {
            new Event1(),
            new Event2(),
            new Event3(),
        };

        // Publish when second subscription responds to Event3 with SomeEvent
        var eventsToPublishSecond = new List<IEvent>
        {
            new Event4(),
            new Event5(),
            new Event6(),
            new Event1(),
            new Event2(),
            new Event1(),
            new Event2(),
            new Event1(),
            new Event2()
        };

        var eventToPublishInResponse = new SomeEvent();

        // Each subscription should receive this list of events in the same order
        // In case of race conditions, some subscriptions should receive events in different order
        var eventsToCheck = new List<IEvent>();

        eventsToCheck.AddRange(eventsToPublishFirst);
        eventsToCheck.Add(eventToPublishInResponse);
        eventsToCheck.AddRange(eventsToPublishSecond);

        _ = Task.Run(async () =>
        {
            firstSubscription
                .Synchronously()
                .Handle<SomeEvent>(_ =>
                {
                    foreach (var eventToPublish in eventsToPublishSecond)
                        firstSubscription.Publish(eventToPublish);
                });

            foreach (var eventToPublish in eventsToPublishFirst)
                firstSubscription.Publish(eventToPublish);

            await firstSubscription.ProcessEventsAsync(cancellation.Token);
        },
        CancellationToken.None);

        _ = Task.Run(async () =>
        {
            secondSubscription
                .Synchronously()
                .Handle<Event3>(_ => secondSubscription.Publish(eventToPublishInResponse));

            await secondSubscription.ProcessEventsAsync(cancellation.Token);
        },
        CancellationToken.None);

        var subscriptionsToEvents = new ConcurrentDictionary<ISubscription, List<IEvent>>();

        for (int i = 0; i < NumberOfSubscriptions; i++)
            subscriptionsToEvents[bus.Subscribe($"Subscription{i}")] = [];

        foreach (var subscriptionToEvents in subscriptionsToEvents)
        {
            _ = Task.Run(async () =>
            {
                var (subscription, events) = subscriptionToEvents;

                subscription
                    .Synchronously()
                    .Handle<Event1>(e => events.Add(e))
                    .Handle<Event2>(e => events.Add(e))
                    .Handle<Event3>(e => events.Add(e))
                    .Handle<Event4>(e => events.Add(e))
                    .Handle<Event5>(e => events.Add(e))
                    .Handle<Event6>(e => events.Add(e))
                    .Handle<SomeEvent>(e => events.Add(e));

                await subscription.ProcessEventsAsync(cancellation.Token);
            },
            CancellationToken.None);
        }

        var allEventsReceived = new ManualResetEventSlim();

        SignalWhenAllEventsReceived(allEventsReceived, subscriptionsToEvents, eventsToCheck.Count, cancellation.Token);

        await Utilities.WaitUntilSetAsync(cancellation.Token, allEventsReceived);

        Assert.Equal(NumberOfSubscriptions, subscriptionsToEvents.Values.Count);

        foreach (var receivedEventsForSubscription in subscriptionsToEvents.Values)
            Assert.Equal(eventsToCheck, receivedEventsForSubscription);
    }

    // Wait for all events to be received by all subscriptions
    private static void SignalWhenAllEventsReceived(
        ManualResetEventSlim allEventsReceived,
        ConcurrentDictionary<ISubscription, List<IEvent>> subscriptionsToEvents,
        int expectedNumberOfEvents,
        CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var allReceived = true;

                foreach (var receivedEventsForSubscription in subscriptionsToEvents.Values)
                {
                    if (receivedEventsForSubscription.Count < expectedNumberOfEvents)
                        allReceived = false;
                }

                if (allReceived)
                    allEventsReceived.Set();

                await Task.Delay(100, cancellationToken);
            }
        },
        CancellationToken.None);
    }

    public void Dispose()
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }
}