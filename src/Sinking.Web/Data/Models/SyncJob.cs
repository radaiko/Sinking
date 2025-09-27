using System.ComponentModel.DataAnnotations;
using Sinking.Core;

namespace Sinking.Web.Data.Models;

/// <summary>
/// Represents a synchronization job between source systems
/// </summary>
public class SyncJob
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Job name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Job name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Source system is required")]
    public SourceSystem SourceSystem { get; set; }
    
    [Required(ErrorMessage = "Target system is required")]
    public SourceSystem TargetSystem { get; set; }
    
    [Required(ErrorMessage = "Source token is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Source token ID must be valid")]
    public int SourceTokenId { get; set; }
    public PersonalAccessToken SourceToken { get; set; } = null!;
    
    [Required(ErrorMessage = "Target token is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Target token ID must be valid")]
    public int TargetTokenId { get; set; }
    public PersonalAccessToken TargetToken { get; set; } = null!;
    
    /// <summary>
    /// Source project/repository identifier
    /// </summary>
    [Required(ErrorMessage = "Source project is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Source project must be between 1 and 200 characters")]
    public string SourceProject { get; set; } = string.Empty;
    
    /// <summary>
    /// Target project/repository identifier
    /// </summary>
    [Required(ErrorMessage = "Target project is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Target project must be between 1 and 200 characters")]
    public string TargetProject { get; set; } = string.Empty;
    
    /// <summary>
    /// Cron expression for scheduled sync
    /// </summary>
    [StringLength(50, ErrorMessage = "Cron expression cannot exceed 50 characters")]
    [RegularExpression(@"^(\*|\d+|\d+-\d+|\*/\d+|\d+/\d+)((\s+(\*|\d+|\d+-\d+|\*/\d+|\d+/\d+)){4})?$", 
        ErrorMessage = "Please enter a valid cron expression")]
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