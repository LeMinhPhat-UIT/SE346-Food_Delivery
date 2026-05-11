using DeliveryService.Consuming;
using DeliveryService.HostedService;
using DeliveryService.Hubs.Implements;
using DeliveryService.Persistences;
using DeliveryService.Repositories.Implements;
using DeliveryService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Extensions;
using DeliveryService.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DeliveryDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DeliveryDbConnectionString")));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")!));
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddEventDispatcher();
builder.Services.AddEventTypeRegistry();

builder.Services.AddSignalR();

builder.Services.AddTransient<IEventHandler<OrderCompletedEvent>, OrderCompletedEventHandler>();
builder.Services.AddHostedService<DeliveryTrackingHostedService>();
builder.Services.AddHostedService<EventConsumerHostedService>();

builder.Services.Configure<DeliveryOption>(builder.Configuration.GetSection("DeliveryOptions"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<TrackingHub>("/hubs/tracking");

app.Run();
