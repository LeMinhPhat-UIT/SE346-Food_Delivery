using AddressService.Mappers;
using AddressService.Persistences;
using AddressService.Repositories.Implements;
using AddressService.Repositories.Interfaces;
using AddressService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AddressDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AddressDbConnectionString");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:AddressDbConnectionString is missing for AddressService.");

    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService.Services.Implements.AddressService>();
builder.Services.AddSingleton<AddressMapper>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await AddressDbInitializer.SeedAsync(scope.ServiceProvider, app.Environment);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
