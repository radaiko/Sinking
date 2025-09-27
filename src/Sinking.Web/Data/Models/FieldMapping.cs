using System.ComponentModel.DataAnnotations;
using Sinking.Core;

namespace Sinking.Web.Data.Models;

/// <summary>
/// Represents custom field mapping between source and target systems
/// </summary>
public class FieldMapping
{
    public int Id { get; set; }
    
    [Required]
    public int SyncJobId { get; set; }
    public SyncJob SyncJob { get; set; } = null!;
    
    /// <summary>
    /// Field type being mapped (Status, Priority, CustomField, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string FieldType { get; set; } = string.Empty;
    
    /// <summary>
    /// Source system field name or identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SourceField { get; set; } = string.Empty;
    
    /// <summary>
    /// Target system field name or identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TargetField { get; set; } = string.Empty;
    
    /// <summary>
    /// Value mapping configuration (JSON format)
    /// Maps source field values to target field values
    /// Example: {"Open": "New", "In Progress": "Active", "Done": "Resolved"}
    /// </summary>
    public string? ValueMapping { get; set; }
    
    /// <summary>
    /// Default value to use if source value is not found in mapping
    /// </summary>
    [StringLength(200)]
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Whether this mapping is required for sync to proceed
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Common field types that can be mapped between systems
/// </summary>
public static class FieldTypes
{
    public const string Status = "Status";
    public const string Priority = "Priority";
    public const string IssueType = "IssueType";
    public const string Labels = "Labels";
    public const string CustomField = "CustomField";
    public const string Assignee = "Assignee";
    public const string Component = "Component";
    public const string Version = "Version";
}