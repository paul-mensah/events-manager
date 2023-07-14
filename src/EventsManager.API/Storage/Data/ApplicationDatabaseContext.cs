using EventsManager.API.Storage.Domain.EventInvitations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventsManager.API.Storage.Data;

public class ApplicationDatabaseContext : DbContext
{
    public ApplicationDatabaseContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<EventInvitation> EventInvitations { get; set; }
}

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDatabaseContext>
{
    public ApplicationDatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        string connectionStrings = configuration.GetConnectionString("DbConnection");
        
        var dbBuilder = new DbContextOptionsBuilder()
            .UseMySQL(connectionStrings);

        return new ApplicationDatabaseContext(dbBuilder.Options);
    }
}