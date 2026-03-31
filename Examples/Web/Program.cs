using Jgss.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .UseEventBus()
    .AddHostedService<RequestBackgroundService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
