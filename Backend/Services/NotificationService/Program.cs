using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Extensions;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consuming;
using NotificationService.HostedService;
using NotificationService.Options;
using NotificationService.Persistences;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddEventDispatcher();
builder.Services.AddEventTypeRegistry();

builder.Services.AddTransient<IEventHandler<OtpSendRequestedEvent>, OtpEmailEventHandler>();
builder.Services.AddTransient<IEventHandler<LockedOutEvent>, LockedOutEventHandler>();
builder.Services.AddTransient<IEventHandler<OrderCompletedEvent>, OrderCompletedEventHandler>();
builder.Services.AddTransient<IEventHandler<ShipperFoundEvent>, ShipperFoundEventHandler>();

builder.Services.AddHostedService<EventConsumerHostedService>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("SmtpSettings"));
//builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDbConnectionString")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
