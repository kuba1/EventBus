using Microsoft.Extensions.DependencyInjection;

using Jgss.EventBus.Implementation;

namespace Jgss.EventBus;

public static class DependencyInjection
{
    public static IServiceCollection UseEventBus(this IServiceCollection serviceCollection) => serviceCollection
        .AddSingleton<ISubscriptionFactory, SubscriptionFactory>()
        .AddSingleton<IBus, Bus>();
}