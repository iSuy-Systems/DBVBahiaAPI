using DBVBahia.Api.Data;
using DBVBahia.Api.Extensions;
using DBVBahia.Data.Context;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DBVBahia.Api.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfig(this IServiceCollection services)
        {
            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;

            });

            services.AddCors(options =>
            {
                options.AddPolicy("Development",
                    builder =>
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());


                options.AddPolicy("Production",
                    builder =>
                        builder
                            .WithMethods("GET")
                            .WithOrigins("http://dbvbahia.com")
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            //.WithHeaders(HeaderNames.ContentType, "x-custom-header")
                            .AllowAnyHeader());
            });

            return services;
        }

        public static IApplicationBuilder UseApiConfig(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("Development");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseCors("Development"); // Usar apenas nas demos => Configuração Ideal: Production
                app.UseHsts();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            AddMigrationsApplicationDBContext(app);
            AddMigrationsDBVBahiaDbContext(app);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/api/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/api/hc-ui";
                    options.ResourcesPath = "/api/hc-ui-resources";

                    options.UseRelativeApiPath = false;
                    options.UseRelativeResourcesPath = false;
                    options.UseRelativeWebhookPath = false;
                });

            });

            return app;
        }

        private static void AddMigrationsApplicationDBContext(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;

                logger.LogInformation("Migrating database...");

                while (!db.CanConnect())
                {
                    logger.LogInformation("Database not ready yet; waiting...");
                    Thread.Sleep(1000);
                }

                try
                {
                    serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                    logger.LogInformation("Database migrated successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
        }

        private static void AddMigrationsDBVBahiaDbContext(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var db = serviceScope.ServiceProvider.GetRequiredService<DBVBahiaDbContext>().Database;

                logger.LogInformation("Migrating database...");

                while (!db.CanConnect())
                {
                    logger.LogInformation("Database not ready yet; waiting...");
                    Thread.Sleep(1000);
                }

                try
                {
                    serviceScope.ServiceProvider.GetRequiredService<DBVBahiaDbContext>().Database.Migrate();
                    logger.LogInformation("Database migrated successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
        }
    }
}