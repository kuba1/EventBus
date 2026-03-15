namespace Jgss.EventBus.IntegrationTests;

internal static class Timeouts
{
    /// <summary>
    /// General timeout for all tests
    /// </summary>
    public const int Test = 30000;

    /// <summary>
    /// Timeout for cases where we wait for something not to happen
    /// </summary>
    public const int NegativeCase = 10000;
}
