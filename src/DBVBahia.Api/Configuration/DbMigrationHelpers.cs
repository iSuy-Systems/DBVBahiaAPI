using DBVBahia.Api.Data;
using DBVBahia.Data.Context;

namespace DBVBahia.Api.Configuration
{

    public static class DbMigrationHelpers
    {
        /// <summary>
        /// Generate migrations before running this method, you can use command bellow:
        /// Nuget package manager: Add-Migration DbInit -context CatalogContext
        /// Dotnet CLI: dotnet ef migrations add DbInit -c CatalogContext
        /// </summary>
        public static async Task EnsureSeedData(WebApplication serviceScope)
        {
            var services = serviceScope.Services.CreateScope().ServiceProvider;
            await EnsureSeedData(services);
        }

        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var contextData = scope.ServiceProvider.GetRequiredService<DBVBahiaDbContext>();

            await DbHealthChecker.TestConnection(context);
            await DbHealthChecker.TestConnection(contextData);

            if (env.IsDevelopment() || env.IsEnvironment("Docker"))
            {
                await context.Database.EnsureCreatedAsync();
                await contextData.Database.EnsureCreatedAsync();
            }
        }
    }

}
