using System.Text.Json;
using EventsManager.API.Middlewares;
using EventsManager.API.Storage.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.API.Extensions;

public static class BuilderExtension
{
    public static WebApplication BuildApplication(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddCors();
        builder.Services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);
        builder.Services.AddCustomServicesAndConfigurations(builder.Configuration);
        builder.Services.AddHealthChecks();

        return builder.Build();
    }

    private static async Task RunMigrations(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            ApplicationDatabaseContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

            int pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).Count();

            if (pendingMigrations >= 1)
            {
                await dbContext.Database.MigrateAsync();
                logger.LogDebug("{pendingMigrations} migrations successfully executed ", pendingMigrations);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured executing pending migrations");
        }
    }

    public static void RunApplication(this WebApplication application)
    {
        // Check if there are pending migrations and execute
        RunMigrations(application.Services).GetAwaiter().GetResult();

        // Configure the HTTP request pipeline.

        application.UseSwagger();
        application.UseSwaggerUI(s => { s.SwaggerEndpoint("/swagger/v1/swagger.json", "Events Manager Api"); });

        application.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials());

        application.UseRouting();
        application.ConfigureGlobalHandler(application.Logger);
        application.UseAuthorization();
        application.MapControllers();

        application.Run();
    }
}