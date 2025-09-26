namespace Sinking.Core;

/// <summary>
/// Represents a field difference between two versions of an issue
/// </summary>
public class FieldDifference
{
    public string FieldName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public DateTime LastModified { get; set; }
    public SourceSystem ModifiedBy { get; set; }
}

/// <summary>
/// Represents the differences between two versions of a UnifiedIssue
/// </summary>
public class IssueDiff
{
    public string IssueId { get; set; } = string.Empty;
    public List<FieldDifference> FieldDifferences { get; set; } = new();
    public List<IssueComment> AddedComments { get; set; } = new();
    public List<IssueComment> ModifiedComments { get; set; } = new();
    public List<IssueComment> RemovedComments { get; set; } = new();
    public List<IssueAttachment> AddedAttachments { get; set; } = new();
    public List<IssueAttachment> RemovedAttachments { get; set; } = new();
    public DateTime DiffGeneratedAt { get; set; } = DateTime.UtcNow;
    
    public bool HasChanges => 
        FieldDifferences.Any() || 
        AddedComments.Any() || 
        ModifiedComments.Any() || 
        RemovedComments.Any() || 
        AddedAttachments.Any() || 
        RemovedAttachments.Any();
}