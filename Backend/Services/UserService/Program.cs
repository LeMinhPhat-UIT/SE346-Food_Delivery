using Messaging.RabbitMq.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using UserService.Consuming;
using UserService.Enums;
using UserService.HostedService;
using UserService.Mappers;
using UserService.Persistences;
using UserService.Repositories.Implements;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole(ApplicationRole.Admin);
    });

    options.AddPolicy("SelfOrAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var isAdmin = context.User.IsInRole(ApplicationRole.Admin);
            if (isAdmin)
                return true;

            var routeId = context.Resource switch
            {
                HttpContext httpContext when httpContext.Request.RouteValues.TryGetValue("id", out var rawId) => rawId?.ToString(),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(routeId))
                return false;

            var claimId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("userId");

            return string.Equals(claimId, routeId, StringComparison.OrdinalIgnoreCase);
        });
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
            throw new InvalidOperationException("JwtSettings:Key is missing for UserService.");

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

builder.Services.AddDbContext<UserDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("UserDbConnectionString");
    options.UseNpgsql(connectionString);
});

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddRabbitMqPublisher();
builder.Services.AddEventDispatcher();
builder.Services.AddEventTypeRegistry();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Services.Implements.UserService>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddTransient<Messaging.Abstractions.Dispatching.IEventHandler<Messaging.Contracts.Events.UserCreatedEvent>, UserCreatedEventHandler>();
builder.Services.AddTransient<Messaging.Abstractions.Dispatching.IEventHandler<Messaging.Contracts.Events.OtpVerifiedEvent>, OtpVerifiedEventHandler>();
builder.Services.AddHostedService<EventConsumerHostedService>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
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
