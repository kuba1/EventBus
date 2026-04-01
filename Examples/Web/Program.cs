using Jgss.EventBus;

using Jgss.EventBus.Examples.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .UseEventBus()
    .AddSingleton<IRequestEventPublisher, RequestEventPublisher>()
    .AddHostedService<TransitionalBackgroundService>()
    .AddHostedService<ResponseBackgroundService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
