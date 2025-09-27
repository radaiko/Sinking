using System.ComponentModel.DataAnnotations;

namespace Sinking.Web.Data.Models;

/// <summary>
/// Represents an execution instance of a sync job with detailed logging
/// </summary>
public class SyncJobExecution
{
    public int Id { get; set; }
    
    [Required]
    public int SyncJobId { get; set; }
    public SyncJob SyncJob { get; set; } = null!;
    
    public SyncJobStatus Status { get; set; } = SyncJobStatus.Queued;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Total number of items to sync
    /// </summary>
    public int TotalItems { get; set; } = 0;
    
    /// <summary>
    /// Number of items successfully synced
    /// </summary>
    public int ProcessedItems { get; set; } = 0;
    
    /// <summary>
    /// Number of items that failed to sync
    /// </summary>
    public int FailedItems { get; set; } = 0;
    
    /// <summary>
    /// General execution log/summary
    /// </summary>
    public string? ExecutionLog { get; set; }
    
    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    public ICollection<SyncJobFailure> Failures { get; set; } = new List<SyncJobFailure>();
}

/// <summary>
/// Represents a specific item that failed to sync with detailed information
/// </summary>
public class SyncJobFailure
{
    public int Id { get; set; }
    
    [Required]
    public int ExecutionId { get; set; }
    public SyncJobExecution Execution { get; set; } = null!;
    
    /// <summary>
    /// Source system item identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SourceItemId { get; set; } = string.Empty;
    
    /// <summary>
    /// Source system item title/summary for display
    /// </summary>
    [StringLength(500)]
    public string? SourceItemTitle { get; set; }
    
    /// <summary>
    /// URL to view the source item
    /// </summary>
    [StringLength(500)]
    public string? SourceItemUrl { get; set; }
    
    /// <summary>
    /// Target system item identifier (if partially created)
    /// </summary>
    [StringLength(100)]
    public string? TargetItemId { get; set; }
    
    /// <summary>
    /// Detailed failure reason for troubleshooting
    /// </summary>
    [Required]
    public string FailureReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Full exception/error details
    /// </summary>
    public string? ErrorDetails { get; set; }
    
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
}