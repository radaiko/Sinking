using Sinking.Core.Models;

namespace Sinking.Core.Interfaces;

public interface ISyncService
{
    SyncServiceType ServiceType { get; }
    Task<IEnumerable<SyncableItem>> GetItemsAsync(string configuration);
    Task<SyncableItem?> GetItemAsync(string configuration, string itemId);
    Task<SyncableItem> CreateItemAsync(string configuration, SyncableItem item);
    Task<SyncableItem> UpdateItemAsync(string configuration, string itemId, SyncableItem item);
    Task<IEnumerable<SyncableAttachment>> GetAttachmentsAsync(string configuration, string itemId);
    Task<SyncableAttachment> AddAttachmentAsync(string configuration, string itemId, SyncableAttachment attachment);
    Task<string> ResolveUserAsync(string configuration, string userIdentifier);
}

public interface ISyncOrchestrator
{
    Task<bool> SyncJobAsync(int jobId);
    Task<bool> SyncJobAsync(SyncJob job);
    Task ScheduleJobsAsync();
}

public interface ISyncRepository
{
    Task<IEnumerable<SyncJob>> GetActiveJobsAsync();
    Task<SyncJob?> GetJobAsync(int id);
    Task<SyncJob> CreateJobAsync(SyncJob job);
    Task<SyncJob> UpdateJobAsync(SyncJob job);
    Task<bool> DeleteJobAsync(int id);
    
    Task<SyncJobRun> CreateJobRunAsync(SyncJobRun run);
    Task<SyncJobRun> UpdateJobRunAsync(SyncJobRun run);
    Task<IEnumerable<SyncJobRun>> GetJobRunsAsync(int jobId, int take = 10);
    
    Task<SyncItem?> GetSyncItemAsync(int jobId, string sourceItemId);
    Task<SyncItem> UpsertSyncItemAsync(SyncItem item);
    Task<IEnumerable<SyncItem>> GetSyncItemsAsync(int jobId);
    
    Task<SyncError> CreateErrorAsync(SyncError error);
    Task<IEnumerable<SyncError>> GetErrorsAsync(int? jobId = null, int take = 50);
}

public class SyncableItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public string? Reporter { get; set; }
    public List<string> Labels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public List<SyncableComment> Comments { get; set; } = new();
}

public class SyncableComment
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SyncableAttachment
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string? DownloadUrl { get; set; }
}