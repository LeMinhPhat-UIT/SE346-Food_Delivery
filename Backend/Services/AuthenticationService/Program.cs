using AuthenticationService.Entities;
using AuthenticationService.Mappers;
using AuthenticationService.Options;
using AuthenticationService.Persistences;
using AuthenticationService.Repositories.Implements;
using AuthenticationService.Repositories.Interfaces;
using AuthenticationService.Services.Implements;
using AuthenticationService.Services.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Messaging.RabbitMq.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthenticationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AuthenticationDbConnectionString");
    //if (string.IsNullOrEmpty(connectionString))
    //    options.UseInMemoryDatabase("AuthenticationDb");
    //else
    options.UseNpgsql(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    var config = builder.Configuration.GetSection("AuthenticationSettings");

    options.Password.RequireDigit = config.GetValue<bool>("PasswordSettings:RequireDigit");
    options.Password.RequireLowercase = config.GetValue<bool>("PasswordSettings:RequireLowercase");
    options.Password.RequireUppercase = config.GetValue<bool>("PasswordSettings:RequireUppercase");
    options.Password.RequireNonAlphanumeric = config.GetValue<bool>("PasswordSettings:RequireNonAlphanumeric");
    options.Password.RequiredLength = config.GetValue<int>("PasswordSettings:RequiredLength");

    options.User.RequireUniqueEmail = config.GetValue<bool>("UserSettings:RequireUniqueEmail");
    options.SignIn.RequireConfirmedEmail = config.GetValue<bool>("SignInSettings:RequireConfirmedEmail");

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(config.GetValue<int>("LockoutSettings:DefaultLockoutTimeSpanInMinutes"));
})
.AddEntityFrameworkStores<AuthenticationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<CustomerRegisterRequestMapper>();
builder.Services.AddRabbitMqPublisher();

builder.Services.Configure<AuthenticationOptions>(builder.Configuration.GetSection("AuthenticationSettings"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddRabbitMqPublisher();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
