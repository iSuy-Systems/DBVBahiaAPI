using DBVBahia.Api.Data;
using DBVBahia.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DBVBahia.Api.Configuration
{

    public static class DbMigrationHelpers
    {
        /// <summary>
        /// Generate migrations before running this method, you can use command bellow:
        /// Nuget package manager: Add-Migration DbInit -context CatalogContext
        /// Dotnet CLI: dotnet ef migrations add DbInit -c CatalogContext
        /// </summary>
        public static void EnsureSeedData(WebApplication serviceScope)
        {
            var services = serviceScope.Services.CreateScope().ServiceProvider;
            EnsureSeedData(services);
        }

        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            applicationDbContext.Database.Migrate();

            var dbContextData = scope.ServiceProvider.GetRequiredService<DBVBahiaDbContext>();
            dbContextData.Database.Migrate();
        }
    }

}
