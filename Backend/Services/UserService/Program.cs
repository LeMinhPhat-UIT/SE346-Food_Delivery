using Messaging.RabbitMq.Extensions;
using Microsoft.EntityFrameworkCore;
using UserService.Consuming;
using UserService.HostedService;
using UserService.Mappers;
using UserService.Persistences;
using UserService.Repositories.Implements;
using UserService.Repositories.Interfaces;
using UserService.Services.Implements;
using UserService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<UserDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("UserDbConnectionString");
    options.UseNpgsql(connectionString);
});

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddEventDispatcher();
builder.Services.AddEventTypeRegistry();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Services.Implements.UserService>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddTransient<Messaging.Abstractions.Dispatching.IEventHandler<Messaging.Contracts.Events.UserCreatedEvent>, UserCreatedEventHandler>();
builder.Services.AddTransient<Messaging.Abstractions.Dispatching.IEventHandler<Messaging.Contracts.Events.OtpVerifiedEvent>, OtpVerifiedEventHandler>();
builder.Services.AddHostedService<EventConsumerHostedService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
