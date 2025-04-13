namespace Jgss.EventBus.UnitTests;

public class EventsTests
{
    record TestEvent : IEvent
    {
        public string TestProperty { get; init; } = string.Empty;
    }

    [Fact(DisplayName = "Given event class when it is instantiated then instantiation succeeds")]
    public void Given_event_class_when_it_is_instantiated_then_instantiation_succeeds()
    {
        var eventInstance = new TestEvent
        {
            TestProperty = "TestValue"
        };

        Assert.Equal("TestValue", eventInstance.TestProperty);
    }
}