using Microsoft.EntityFrameworkCore;

namespace AddressService.Persistences
{
    public static class AddressDbInitializer
    {
        private const string ImportDataFileName = "postgres_ImportData_vn_units.sql";

        public static async Task SeedAsync(IServiceProvider serviceProvider, IWebHostEnvironment environment)
        {
            var dbContext = serviceProvider.GetRequiredService<AddressDbContext>();

            await dbContext.Database.EnsureCreatedAsync();

            if (await dbContext.Wards.AnyAsync())
                return;

            var importScriptPath = ResolveImportScriptPath(environment);
            var importScript = await File.ReadAllTextAsync(importScriptPath);

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    "TRUNCATE TABLE wards, provinces, administrative_units, administrative_regions RESTART IDENTITY CASCADE");
                await dbContext.Database.ExecuteSqlRawAsync(importScript);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string ResolveImportScriptPath(IWebHostEnvironment environment)
        {
            var candidates = new[]
            {
                Path.Combine(environment.ContentRootPath, "Sql", ImportDataFileName),
                Path.Combine(AppContext.BaseDirectory, "Sql", ImportDataFileName)
            };

            var path = candidates.FirstOrDefault(File.Exists);
            if (path is not null)
                return path;

            throw new FileNotFoundException($"Address import script '{ImportDataFileName}' was not found.", ImportDataFileName);
        }
    }
}
