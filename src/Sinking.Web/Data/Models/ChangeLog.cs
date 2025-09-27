using System.ComponentModel.DataAnnotations;

namespace Sinking.Web.Data.Models;

/// <summary>
/// Tracks all changes made through the web UI for CRA compliance
/// </summary>
public class ChangeLog
{
    public int Id { get; set; }
    
    /// <summary>
    /// Type of entity that was changed (PAT, SyncJob, FieldMapping, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the entity that was changed
    /// </summary>
    [Required]
    public int EntityId { get; set; }
    
    /// <summary>
    /// Type of change (Create, Update, Delete, Activate, Deactivate, etc.)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ChangeType { get; set; } = string.Empty;
    
    /// <summary>
    /// User who made the change
    /// </summary>
    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    /// <summary>
    /// Detailed description of what changed
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Before values (JSON format for complex objects)
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// After values (JSON format for complex objects)
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// IP address of the user making the change
    /// </summary>
    [StringLength(45)]
    public string? IPAddress { get; set; }
    
    /// <summary>
    /// User agent of the browser/client making the change
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Common change types for audit logging
/// </summary>
public static class ChangeTypes
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Activate = "Activate";
    public const string Deactivate = "Deactivate";
    public const string Pause = "Pause";
    public const string Resume = "Resume";
    public const string TestConnection = "TestConnection";
    public const string RunSync = "RunSync";
    public const string CancelSync = "CancelSync";
}