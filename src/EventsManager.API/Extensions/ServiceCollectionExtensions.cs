using System.Reflection;
using Arch.EntityFrameworkCore.UnitOfWork;
using Elasticsearch.Net;
using EventsManager.API.Configurations;
using EventsManager.API.Services.Implementations;
using EventsManager.API.Services.Interfaces;
using EventsManager.API.Storage.Data;
using EventsManager.API.Storage.Repositories.Implementations;
using EventsManager.API.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Nest;
using StackExchange.Redis;

namespace EventsManager.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Events Manager Api",
                Version = "v1",
                Description = "Events Manager Api",
                Contact = new OpenApiContact
                {
                    Name = "Paul Mensah",
                    Email = "paulmensah1409@gmail.com"
                }
            });
            
            c.ResolveConflictingActions(resolver => resolver.First());
            c.EnableAnnotations();
            
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    private static void AddElasticSearch(this IServiceCollection services, Action<ElasticsearchConfig> elasticsearchConfig)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        services.Configure(elasticsearchConfig);

        var elasticsearchConfiguration = new ElasticsearchConfig();
        elasticsearchConfig.Invoke(elasticsearchConfiguration);

        var pool = new SingleNodeConnectionPool(new Uri(elasticsearchConfiguration.Url));
        var connectionSettings = new ConnectionSettings(pool)
            .DefaultIndex(elasticsearchConfiguration.Index);
        connectionSettings.PrettyJson();
        connectionSettings.DisableDirectStreaming();
        connectionSettings.EnableApiVersioningHeader();
        
        var elasticClient = new ElasticClient(connectionSettings);
        var elasticLowLevelClient = new ElasticLowLevelClient(connectionSettings);

        services.AddSingleton<IElasticClient>(elasticClient);
        services.AddSingleton<IElasticLowLevelClient>(elasticLowLevelClient);
        services.AddSingleton<IElasticsearchService, ElasticsearchService>();
    }

    private static void AddRedisCache(this IServiceCollection services, Action<RedisConfig> redisConfig)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        services.Configure(redisConfig);

        var redisConfiguration = new RedisConfig();
        redisConfig.Invoke(redisConfiguration);
        
        var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { redisConfiguration.BaseUrl },
            AllowAdmin = true,
            AbortOnConnectFail = false,
            ReconnectRetryPolicy = new LinearRetry(500),
            DefaultDatabase = redisConfiguration.Database
        });

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        services.AddSingleton<IRedisService, RedisService>();
    }

    private static void AddMySqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDatabaseContext>(options =>
        {
            options.UseMySQL(configuration.GetConnectionString("DbConnection") ?? 
                             throw new InvalidOperationException());
        }, ServiceLifetime.Transient).AddUnitOfWork<ApplicationDatabaseContext>();
    }
    
    public static void AddCustomServicesAndConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // Services
        services.AddElasticSearch(c => configuration.GetSection(nameof(ElasticsearchConfig)).Bind(c));
        services.AddMySqlDatabase(configuration);
        services.AddRedisCache(c => configuration.GetSection(nameof(RedisConfig)).Bind(c));
        services.AddScoped<IEventInvitationRepository, EventInvitationRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IInvitationService, InvitationService>();
    }
    
    
}