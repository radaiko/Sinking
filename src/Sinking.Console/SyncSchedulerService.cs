using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sinking.Core.Interfaces;

namespace Sinking.Console;

public class SyncSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncSchedulerService> _logger;
    private readonly TimeSpan _schedulerInterval = TimeSpan.FromMinutes(1); // Check every minute

    public SyncSchedulerService(IServiceProvider serviceProvider, ILogger<SyncSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync Scheduler Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<ISyncOrchestrator>();
                
                await orchestrator.ScheduleJobsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scheduling sync jobs: {Error}", ex.Message);
            }

            await Task.Delay(_schedulerInterval, stoppingToken);
        }

        _logger.LogInformation("Sync Scheduler Service stopped");
    }
}