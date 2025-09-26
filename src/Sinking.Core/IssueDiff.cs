namespace Sinking.Core;

/// <summary>
/// Represents a field difference between two versions of an issue
/// </summary>
public class FieldDifference
{
    /// <summary>
    /// Gets or sets the name of the field that changed
    /// </summary>
    public string FieldName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the previous value of the field
    /// </summary>
    public object? OldValue { get; set; }
    
    /// <summary>
    /// Gets or sets the new value of the field
    /// </summary>
    public object? NewValue { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the field was last modified
    /// </summary>
    public DateTime LastModified { get; set; }
    
    /// <summary>
    /// Gets or sets the source system that made the modification
    /// </summary>
    public SourceSystem ModifiedBy { get; set; }
}

/// <summary>
/// Represents the differences between two versions of a UnifiedIssue
/// </summary>
public class IssueDiff
{
    /// <summary>
    /// Gets or sets the unique identifier of the issue being compared
    /// </summary>
    public string IssueId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the list of field differences
    /// </summary>
    public List<FieldDifference> FieldDifferences { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of comments that were added
    /// </summary>
    public List<IssueComment> AddedComments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of comments that were modified
    /// </summary>
    public List<IssueComment> ModifiedComments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of comments that were removed
    /// </summary>
    public List<IssueComment> RemovedComments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of attachments that were added
    /// </summary>
    public List<IssueAttachment> AddedAttachments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of attachments that were removed
    /// </summary>
    public List<IssueAttachment> RemovedAttachments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the timestamp when the diff was generated
    /// </summary>
    public DateTime DiffGeneratedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets a value indicating whether there are any changes in this diff
    /// </summary>
    public bool HasChanges => 
        (FieldDifferences?.Any() ?? false) || 
        (AddedComments?.Any() ?? false) || 
        (ModifiedComments?.Any() ?? false) || 
        (RemovedComments?.Any() ?? false) || 
        (AddedAttachments?.Any() ?? false) || 
        (RemovedAttachments?.Any() ?? false);
}