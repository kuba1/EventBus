namespace Jgss.EventBus.UnitTests;

record TestEvent : IEvent
{
    public string TestProperty { get; set; } = string.Empty;
}

public class EventBusTests
{
    [Fact(DisplayName = "Given event class when it is instantiated then instantiation succeeds")]
    public void Given_event_class_when_it_is_instantiated_then_instantiation_succeeds()
    {
        var eventInstance = new TestEvent
        {
            TestProperty = "TestValue"
        };

        eventInstance.TestProperty.Should().Be("TestValue");
    }
}