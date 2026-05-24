using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Extensions;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consuming;
using NotificationService.HostedService;
using NotificationService.Mappers;
using NotificationService.Options;
using NotificationService.Persistences;
using NotificationService.Repositories.Implements;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Implements;
using NotificationService.Services.Interfaces;
using Scalar.AspNetCore;

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
builder.Services.AddTransient<IEventHandler<DeliveryMilestoneEvent>, DeliveryMilestoneEventHandler>();
builder.Services.AddTransient<IEventHandler<MerchantRequestReviewedEvent>, MerchantRequestReviewedEventHandler>();
builder.Services.AddTransient<IEventHandler<ShipperRequestReviewedEvent>, ShipperRequestReviewedEventHandler>();
builder.Services.AddTransient<IPushNotificationService, PushNotificationService>();
builder.Services.AddTransient<UserDeviceMapper>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.Implements.NotificationService>();

builder.Services.AddHostedService<EventConsumerHostedService>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("SmtpSettings"));
//builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDbConnectionString")));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .AddPreferredSecuritySchemes("BearerAuth")
        .AddHttpAuthentication("BearerAuth", auth => { }));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
