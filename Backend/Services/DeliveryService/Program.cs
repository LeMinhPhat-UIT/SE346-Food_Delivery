using DeliveryService.Consuming;
using DeliveryService.HostedService;
using DeliveryService.Hubs.Implements;
using DeliveryService.Integrations;
using DeliveryService.Mappers;
using DeliveryService.Options;
using DeliveryService.Persistences;
using DeliveryService.Services.Implements;
using DeliveryService.Repositories.Implements;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<DeliveryDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DeliveryDbConnectionString")));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin", "ADMIN");
    });

    options.AddPolicy("ShipperOrAdmin", policy =>
    {
        policy.RequireRole("Shipper", "SHIPPER", "Admin", "ADMIN");
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["JwtSettings:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("JwtSettings:Key is missing for DeliveryService.");

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/tracking"))
                context.Token = accessToken;

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")!));
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryService.Services.Implements.DeliveryService>();
builder.Services.AddScoped<IDeliveryFeePolicyService, DeliveryFeePolicyService>();
builder.Services.AddScoped<IDeliveryEstimator, DeliveryEstimator>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();
builder.Services.AddSingleton<DeliveryMapper>();

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddRabbitMqPublisher();
builder.Services.AddEventDispatcher();
builder.Services.AddEventTypeRegistry();

builder.Services.AddSignalR();

builder.Services.AddTransient<IEventHandler<OrderReadyForPickupEvent>, OrderReadyForPickupEventHandler>();
builder.Services.AddTransient<IEventHandler<OrderCompletedEvent>, OrderReadyForPickupEventHandler>();
builder.Services.AddHostedService<DeliveryTrackingHostedService>();
builder.Services.AddHostedService<AssignmentOfferExpirationHostedService>();
builder.Services.AddHostedService<EventConsumerHostedService>();

builder.Services.AddOptions<DeliveryOption>()
    .Bind(builder.Configuration.GetSection("DeliveryOptions"))
    .Validate(options => options.TryGetRedisGeoUnit(out _), DeliveryGeoUnitParser.SupportedValuesMessage)
    .ValidateOnStart();
builder.Services.Configure<OpenRouteServiceOptions>(options =>
{
    builder.Configuration.GetSection(OpenRouteServiceOptions.SectionName).Bind(options);

    var apiKey = builder.Configuration["OpenRouteService_ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
        options.ApiKey = apiKey;

    var url = builder.Configuration["OpenRouteService_Url"];
    if (!string.IsNullOrWhiteSpace(url))
        options.Url = url;

    var profile = builder.Configuration["OpenRouteService_Profile"];
    if (!string.IsNullOrWhiteSpace(profile))
        options.Profile = profile;

    var timeoutSeconds = builder.Configuration["OpenRouteService_TimeoutSeconds"];
    if (int.TryParse(timeoutSeconds, out var timeout))
        options.TimeoutSeconds = timeout;
});

builder.Services.AddHttpClient<IOpenRouteServiceClient, OpenRouteServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<OpenRouteServiceOptions>>().Value;
    var url = string.IsNullOrWhiteSpace(options.Url)
        ? "https://api.openrouteservice.org"
        : options.Url.Trim();

    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(Math.Max(options.TimeoutSeconds, 1));
});

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    var baseUrl = builder.Configuration["UserService:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        baseUrl = "http://user-service:8080";

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
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
app.MapHub<TrackingHub>("/hubs/tracking");

app.Run();
