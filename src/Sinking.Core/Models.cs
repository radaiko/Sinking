using System.ComponentModel.DataAnnotations;

namespace Sinking.Core.Models;

public class SyncJob
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public SyncServiceType SourceService { get; set; }
    public SyncServiceType TargetService { get; set; }
    
    public string SourceConfiguration { get; set; } = string.Empty; // JSON configuration
    public string TargetConfiguration { get; set; } = string.Empty; // JSON configuration
    
    public bool IsEnabled { get; set; } = true;
    public bool IsBidirectional { get; set; } = false;
    
    [MaxLength(100)]
    public string? CronSchedule { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<FieldMapping> FieldMappings { get; set; } = new();
    public List<UserMapping> UserMappings { get; set; } = new();
}

public class SyncJobRun
{
    public int Id { get; set; }
    public int SyncJobId { get; set; }
    
    public SyncJobStatus Status { get; set; } = SyncJobStatus.Running;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    
    public int ItemsProcessed { get; set; }
    public int ItemsSucceeded { get; set; }
    public int ItemsFailed { get; set; }
    
    public string? LogMessage { get; set; }
    
    // Navigation properties
    public SyncJob SyncJob { get; set; } = null!;
}

public class SyncItem
{
    public int Id { get; set; }
    public int SyncJobId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SourceItemId { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? TargetItemId { get; set; }
    
    public string? SourceData { get; set; } // JSON snapshot
    public string? TargetData { get; set; } // JSON snapshot
    
    public DateTime LastSyncTime { get; set; } = DateTime.UtcNow;
    public DateTime SourceLastModified { get; set; }
    public DateTime? TargetLastModified { get; set; }
    
    public SyncItemStatus Status { get; set; } = SyncItemStatus.Pending;
    
    // Navigation properties
    public SyncJob SyncJob { get; set; } = null!;
}

public class SyncError
{
    public int Id { get; set; }
    public int? SyncJobId { get; set; }
    
    [Required]
    public string ErrorMessage { get; set; } = string.Empty;
    
    public string? StackTrace { get; set; }
    public string? Context { get; set; } // Additional context as JSON
    
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public SyncJob? SyncJob { get; set; }
}

public class FieldMapping
{
    public int Id { get; set; }
    public int SyncJobId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SourceField { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string TargetField { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? TransformExpression { get; set; } // Optional transformation logic
    
    public bool IsRequired { get; set; } = false;
    
    // Navigation properties
    public SyncJob SyncJob { get; set; } = null!;
}

public class UserMapping
{
    public int Id { get; set; }
    public int SyncJobId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string SourceUser { get; set; } = string.Empty; // Username or email
    
    [Required]
    [MaxLength(200)]
    public string TargetUser { get; set; } = string.Empty; // Username or email
    
    // Navigation properties
    public SyncJob SyncJob { get; set; } = null!;
}

public enum SyncServiceType
{
    GitHub = 1,
    Jira = 2,
    AzureDevOps = 3
}

public enum SyncJobStatus
{
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

public enum SyncItemStatus
{
    Pending = 1,
    Synced = 2,
    Failed = 3,
    Conflict = 4,
    Skipped = 5
}
