using DeliveryService.Consuming;
using DeliveryService.Helpers;
using DeliveryService.HostedService;
using DeliveryService.Hubs.Implements;
using DeliveryService.Persistences;
using DeliveryService.Repositories.Implements;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Extensions;
using DeliveryService.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DeliveryDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DeliveryDbConnectionString")));
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
});
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")!));
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryService.Services.Implements.DeliveryService>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();
builder.Services.AddScoped<FirebaseStorageHelper>();

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddRabbitMqPublisher();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TrackingHub>("/hubs/tracking");

app.Run();
