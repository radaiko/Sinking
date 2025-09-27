using System.ComponentModel.DataAnnotations;
using Sinking.Core;

namespace Sinking.Web.Data.Models;

/// <summary>
/// Represents a synchronization job between source systems
/// </summary>
public class SyncJob
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public SourceSystem SourceSystem { get; set; }
    
    [Required]
    public SourceSystem TargetSystem { get; set; }
    
    [Required]
    public int SourceTokenId { get; set; }
    public PersonalAccessToken SourceToken { get; set; } = null!;
    
    [Required]
    public int TargetTokenId { get; set; }
    public PersonalAccessToken TargetToken { get; set; } = null!;
    
    /// <summary>
    /// Source project/repository identifier
    /// </summary>
    [Required]
    [StringLength(200)]
    public string SourceProject { get; set; } = string.Empty;
    
    /// <summary>
    /// Target project/repository identifier
    /// </summary>
    [Required]
    [StringLength(200)]
    public string TargetProject { get; set; } = string.Empty;
    
    /// <summary>
    /// Cron expression for scheduled sync
    /// </summary>
    [StringLength(50)]
    public string? CronExpression { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsPaused { get; set; } = false;
    
    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last successful sync timestamp
    /// </summary>
    public DateTime? LastSyncAt { get; set; }
    
    /// <summary>
    /// Next scheduled sync timestamp
    /// </summary>
    public DateTime? NextSyncAt { get; set; }
    
    public ICollection<SyncJobExecution> Executions { get; set; } = new List<SyncJobExecution>();
    public ICollection<FieldMapping> FieldMappings { get; set; } = new List<FieldMapping>();
}

/// <summary>
/// Status of a sync job execution
/// </summary>
public enum SyncJobStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}