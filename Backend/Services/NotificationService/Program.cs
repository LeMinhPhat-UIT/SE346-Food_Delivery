using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Extensions;
using Messaging.RabbitMq.Options;
using NotificationService.Consuming;
using NotificationService.HostedService;
using NotificationService.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddEventDispatcher();
builder.Services.AddEventTypeRegistry();

builder.Services.AddTransient<IEventHandler<OtpSendRequestedEvent>, OtpEmailEventHandler>();
builder.Services.AddTransient<IEventHandler<LockedOutEvent>, LockedOutEventHandler>();

builder.Services.AddHostedService<EventConsumerHostedService>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("SmtpSettings"));
//builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
