namespace Jgss.EventBus.UnitTests;

internal static class Timeouts
{
    /// <summary>
    /// General timeout for all tests
    /// </summary>
    public const int Test = 10000;

    /// <summary>
    /// Timeout for cases where we wait for something not to happen
    /// </summary>
    public const int NegativeCase = 2000;
}
