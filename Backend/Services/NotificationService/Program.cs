using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Consuming;
using NotificationService.HostedService;
using NotificationService.Integrations;
using NotificationService.Mappers;
using NotificationService.Options;
using NotificationService.Persistences;
using NotificationService.Repositories.Implements;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Implements;
using NotificationService.Services.Interfaces;
using Scalar.AspNetCore;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin", "ADMIN");
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["JwtSettings:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("JwtSettings:Key is missing for NotificationService.");

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

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
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    var baseUrl = builder.Configuration["UserService:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        baseUrl = "http://user-service:8080";

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
});

builder.Services.AddHostedService<EventConsumerHostedService>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("SmtpSettings"));
//builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDbConnectionString")));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

if (FirebaseApp.DefaultInstance is null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
}

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
