using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;
using Sinking.Core.Interfaces;
using Sinking.Core.Models;
using System.Text.Json;

namespace Sinking.Core.Services;

public class SyncOrchestrator : ISyncOrchestrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISyncRepository _repository;
    private readonly ILogger<SyncOrchestrator> _logger;

    public SyncOrchestrator(
        IServiceProvider serviceProvider, 
        ISyncRepository repository, 
        ILogger<SyncOrchestrator> logger)
    {
        _serviceProvider = serviceProvider;
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> SyncJobAsync(int jobId)
    {
        var job = await _repository.GetJobAsync(jobId);
        if (job == null)
        {
            _logger.LogWarning("Sync job {JobId} not found", jobId);
            return false;
        }

        return await SyncJobAsync(job);
    }

    public async Task<bool> SyncJobAsync(SyncJob job)
    {
        var jobRun = new SyncJobRun
        {
            SyncJobId = job.Id,
            Status = SyncJobStatus.Running,
            StartTime = DateTime.UtcNow
        };

        await _repository.CreateJobRunAsync(jobRun);

        try
        {
            _logger.LogInformation("Starting sync job {JobId} - {JobName}", job.Id, job.Name);

            // Get the appropriate sync services
            var sourceService = GetSyncService(job.SourceService);
            var targetService = GetSyncService(job.TargetService);

            if (sourceService == null || targetService == null)
            {
                throw new InvalidOperationException($"Unable to create sync services for job {job.Id}");
            }

            // Get items from source
            var sourceItems = await sourceService.GetItemsAsync(job.SourceConfiguration);
            
            int processed = 0, succeeded = 0, failed = 0;

            foreach (var sourceItem in sourceItems)
            {
                processed++;
                
                try
                {
                    await SyncItemAsync(job, sourceItem, sourceService, targetService);
                    succeeded++;
                    
                    // Handle bidirectional sync
                    if (job.IsBidirectional)
                    {
                        await SyncItemReverseAsync(job, sourceItem, sourceService, targetService);
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogError(ex, "Failed to sync item {ItemId} for job {JobId}", sourceItem.Id, job.Id);
                    
                    await _repository.CreateErrorAsync(new SyncError
                    {
                        SyncJobId = job.Id,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace,
                        Context = JsonSerializer.Serialize(new { ItemId = sourceItem.Id, JobId = job.Id })
                    });
                }
            }

            // Update job run status
            jobRun.Status = SyncJobStatus.Completed;
            jobRun.EndTime = DateTime.UtcNow;
            jobRun.ItemsProcessed = processed;
            jobRun.ItemsSucceeded = succeeded;
            jobRun.ItemsFailed = failed;
            jobRun.LogMessage = $"Processed {processed} items. Succeeded: {succeeded}, Failed: {failed}";

            await _repository.UpdateJobRunAsync(jobRun);

            _logger.LogInformation("Completed sync job {JobId}. Processed: {Processed}, Succeeded: {Succeeded}, Failed: {Failed}", 
                job.Id, processed, succeeded, failed);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync job {JobId} failed with error: {Error}", job.Id, ex.Message);

            jobRun.Status = SyncJobStatus.Failed;
            jobRun.EndTime = DateTime.UtcNow;
            jobRun.LogMessage = ex.Message;

            await _repository.UpdateJobRunAsync(jobRun);

            await _repository.CreateErrorAsync(new SyncError
            {
                SyncJobId = job.Id,
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace,
                Context = JsonSerializer.Serialize(new { JobId = job.Id })
            });

            return false;
        }
    }

    public async Task ScheduleJobsAsync()
    {
        var jobs = await _repository.GetActiveJobsAsync();
        var now = DateTime.UtcNow;

        foreach (var job in jobs.Where(j => !string.IsNullOrEmpty(j.CronSchedule)))
        {
            try
            {
                var schedule = CrontabSchedule.Parse(job.CronSchedule);
                var lastRuns = await _repository.GetJobRunsAsync(job.Id, 1);
                var lastRun = lastRuns.FirstOrDefault();

                var nextRun = schedule.GetNextOccurrence(lastRun?.StartTime ?? now.AddMinutes(-1));
                
                if (now >= nextRun)
                {
                    _logger.LogInformation("Triggering scheduled sync job {JobId} - {JobName}", job.Id, job.Name);
                    _ = Task.Run(() => SyncJobAsync(job)); // Fire and forget
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled job {JobId}: {Error}", job.Id, ex.Message);
            }
        }
    }

    private async Task SyncItemAsync(SyncJob job, SyncableItem sourceItem, ISyncService sourceService, ISyncService targetService)
    {
        // Get existing sync item
        var syncItem = await _repository.GetSyncItemAsync(job.Id, sourceItem.Id);
        
        if (syncItem != null)
        {
            // Check if source item has been updated since last sync
            if (sourceItem.UpdatedAt <= syncItem.SourceLastModified)
            {
                return; // No changes since last sync
            }

            // Handle conflict resolution by timestamp
            if (syncItem.TargetLastModified.HasValue && 
                syncItem.TargetLastModified.Value > syncItem.SourceLastModified)
            {
                _logger.LogWarning("Potential conflict detected for item {ItemId}. Target modified after source.", sourceItem.Id);
                // In this implementation, source wins. You could implement different conflict resolution strategies.
            }
        }

        // Apply field mappings and user mappings
        var mappedItem = ApplyMappings(sourceItem, job);

        SyncableItem targetItem;
        
        if (syncItem?.TargetItemId != null)
        {
            // Update existing item
            targetItem = await targetService.UpdateItemAsync(job.TargetConfiguration, syncItem.TargetItemId, mappedItem);
        }
        else
        {
            // Create new item
            targetItem = await targetService.CreateItemAsync(job.TargetConfiguration, mappedItem);
        }

        // Update sync item record
        var newSyncItem = new SyncItem
        {
            SyncJobId = job.Id,
            SourceItemId = sourceItem.Id,
            TargetItemId = targetItem.Id,
            SourceData = JsonSerializer.Serialize(sourceItem),
            TargetData = JsonSerializer.Serialize(targetItem),
            LastSyncTime = DateTime.UtcNow,
            SourceLastModified = sourceItem.UpdatedAt,
            TargetLastModified = targetItem.UpdatedAt,
            Status = SyncItemStatus.Synced
        };

        await _repository.UpsertSyncItemAsync(newSyncItem);
    }

    private async Task SyncItemReverseAsync(SyncJob job, SyncableItem sourceItem, ISyncService sourceService, ISyncService targetService)
    {
        // This would implement bidirectional sync logic
        // For brevity, this is a placeholder - you'd implement similar logic but in reverse
        await Task.CompletedTask;
    }

    private SyncableItem ApplyMappings(SyncableItem sourceItem, SyncJob job)
    {
        var mappedItem = new SyncableItem
        {
            Id = sourceItem.Id,
            Title = sourceItem.Title,
            Description = sourceItem.Description,
            Status = sourceItem.Status,
            Assignee = sourceItem.Assignee,
            Reporter = sourceItem.Reporter,
            Labels = new List<string>(sourceItem.Labels),
            CreatedAt = sourceItem.CreatedAt,
            UpdatedAt = sourceItem.UpdatedAt,
            CustomFields = new Dictionary<string, object>(sourceItem.CustomFields)
        };

        // Apply field mappings
        foreach (var mapping in job.FieldMappings)
        {
            // This is a simplified field mapping - in practice, you'd need more sophisticated logic
            if (sourceItem.CustomFields.TryGetValue(mapping.SourceField, out var value))
            {
                mappedItem.CustomFields[mapping.TargetField] = value;
            }
        }

        // Apply user mappings
        foreach (var userMapping in job.UserMappings)
        {
            if (mappedItem.Assignee == userMapping.SourceUser)
            {
                mappedItem.Assignee = userMapping.TargetUser;
            }
            if (mappedItem.Reporter == userMapping.SourceUser)
            {
                mappedItem.Reporter = userMapping.TargetUser;
            }
        }

        return mappedItem;
    }

    private ISyncService? GetSyncService(SyncServiceType serviceType)
    {
        return serviceType switch
        {
            SyncServiceType.GitHub => _serviceProvider.GetService<GitHubSyncService>(),
            SyncServiceType.Jira => _serviceProvider.GetService<JiraSyncService>(),
            SyncServiceType.AzureDevOps => _serviceProvider.GetService<AzureDevOpsSyncService>(),
            _ => null
        };
    }
}