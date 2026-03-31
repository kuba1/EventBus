using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Jgss.EventBus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .UseEventBus()
    .AddHostedService<FirstBackgroundService>()
    .AddHostedService<SecondBackgroundService>();

using var host = builder.Build();

await host.RunAsync();
