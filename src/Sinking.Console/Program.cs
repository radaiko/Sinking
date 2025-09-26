using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sinking.Console;
using Sinking.Core.Interfaces;
using Sinking.Core.Services;
using Sinking.Data;
using Sinking.Data.Repositories;

// Create host builder for console app with DI
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configure Entity Framework with SQLite
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=data/sinking.db";
        services.AddDbContext<SinkingDbContext>(options => options.UseSqlite(connectionString));

        // Register repositories
        services.AddScoped<ISyncRepository, SyncRepository>();

        // Register sync services
        services.AddHttpClient<GitHubSyncService>();
        services.AddHttpClient<JiraSyncService>();
        services.AddHttpClient<AzureDevOpsSyncService>();

        // Register orchestrator
        services.AddScoped<ISyncOrchestrator, SyncOrchestrator>();

        // Register the main service
        services.AddHostedService<SyncSchedulerService>();
    })
    .Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SinkingDbContext>();
    context.Database.EnsureCreated();
}

// Start the host
await host.RunAsync();
