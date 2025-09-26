using System.Text.Json;

namespace Sinking.Core;

/// <summary>
/// Unified representation of an issue from different source systems (Jira, GitHub, Azure DevOps)
/// </summary>
public class UnifiedIssue
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueStatus Status { get; set; } = IssueStatus.New;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    public string Assignee { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<IssueComment> Comments { get; set; } = new();
    public List<IssueAttachment> Attachments { get; set; } = new();
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public SourceSystem SourceSystem { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTime LastSyncAt { get; set; }
    
    /// <summary>
    /// Creates a UnifiedIssue from a Jira work item
    /// </summary>
    public static UnifiedIssue FromJira(object jiraIssue)
    {
        // For now, this is a placeholder that accepts a dictionary-like structure
        if (jiraIssue is Dictionary<string, object> jira)
        {
            var issue = new UnifiedIssue
            {
                Id = jira.GetValueOrDefault("id", "")?.ToString() ?? "",
                Title = jira.GetValueOrDefault("summary", "")?.ToString() ?? "",
                Description = jira.GetValueOrDefault("description", "")?.ToString() ?? "",
                Status = MapJiraStatus(jira.GetValueOrDefault("status", "")?.ToString() ?? ""),
                Priority = MapJiraPriority(jira.GetValueOrDefault("priority", "")?.ToString() ?? ""),
                Assignee = jira.GetValueOrDefault("assignee", "")?.ToString() ?? "",
                SourceSystem = SourceSystem.Jira,
                SourceId = jira.GetValueOrDefault("key", "")?.ToString() ?? "",
                SourceUrl = jira.GetValueOrDefault("url", "")?.ToString() ?? "",
                CreatedAt = ParseDateTime(jira.GetValueOrDefault("created", DateTime.UtcNow)),
                UpdatedAt = ParseDateTime(jira.GetValueOrDefault("updated", DateTime.UtcNow)),
                LastSyncAt = DateTime.UtcNow
            };
            
            // Map labels
            if (jira.TryGetValue("labels", out var labelsObj) && labelsObj is List<object> labels)
            {
                issue.Labels = labels.Select(l => l.ToString() ?? "").Where(l => !string.IsNullOrEmpty(l)).ToList();
            }
            
            // Map comments
            if (jira.TryGetValue("comments", out var commentsObj) && commentsObj is List<object> comments)
            {
                issue.Comments = comments.Select(c => MapJiraComment(c)).Where(c => c != null).ToList()!;
            }
            
            // Map attachments
            if (jira.TryGetValue("attachments", out var attachmentsObj) && attachmentsObj is List<object> attachments)
            {
                issue.Attachments = attachments.Select(a => MapJiraAttachment(a)).Where(a => a != null).ToList()!;
            }
            
            // Map custom fields
            if (jira.TryGetValue("customFields", out var customFieldsObj) && customFieldsObj is Dictionary<string, object> customFields)
            {
                issue.CustomFields = new Dictionary<string, object>(customFields);
            }
            
            return issue;
        }
        
        throw new ArgumentException("Invalid Jira issue format", nameof(jiraIssue));
    }
    
    /// <summary>
    /// Creates a UnifiedIssue from a GitHub issue
    /// </summary>
    public static UnifiedIssue FromGitHub(object githubIssue)
    {
        if (githubIssue is Dictionary<string, object> github)
        {
            var issue = new UnifiedIssue
            {
                Id = github.GetValueOrDefault("id", "").ToString() ?? "",
                Title = github.GetValueOrDefault("title", "").ToString() ?? "",
                Description = github.GetValueOrDefault("body", "").ToString() ?? "",
                Status = MapGitHubStatus(github.GetValueOrDefault("state", "").ToString() ?? ""),
                Priority = IssuePriority.Medium, // GitHub doesn't have built-in priority
                Assignee = ExtractGitHubAssignee(github.GetValueOrDefault("assignee", null)),
                SourceSystem = SourceSystem.GitHub,
                SourceId = github.GetValueOrDefault("number", "").ToString() ?? "",
                SourceUrl = github.GetValueOrDefault("html_url", "").ToString() ?? "",
                CreatedAt = ParseDateTime(github.GetValueOrDefault("created_at", DateTime.UtcNow)),
                UpdatedAt = ParseDateTime(github.GetValueOrDefault("updated_at", DateTime.UtcNow)),
                LastSyncAt = DateTime.UtcNow
            };
            
            // Map labels
            if (github.TryGetValue("labels", out var labelsObj) && labelsObj is List<object> labels)
            {
                issue.Labels = labels.Select(l => ExtractGitHubLabelName(l)).Where(l => !string.IsNullOrEmpty(l)).ToList();
            }
            
            // Map comments
            if (github.TryGetValue("comments", out var commentsObj) && commentsObj is List<object> comments)
            {
                issue.Comments = comments.Select(c => MapGitHubComment(c)).Where(c => c != null).ToList()!;
            }
            
            // Map custom fields from metadata
            if (github.TryGetValue("customFields", out var customFieldsObj) && customFieldsObj is Dictionary<string, object> customFields)
            {
                issue.CustomFields = new Dictionary<string, object>(customFields);
            }
            
            return issue;
        }
        
        throw new ArgumentException("Invalid GitHub issue format", nameof(githubIssue));
    }
    
    /// <summary>
    /// Creates a UnifiedIssue from an Azure DevOps work item
    /// </summary>
    public static UnifiedIssue FromAzureDevOps(object azureWorkItem)
    {
        if (azureWorkItem is Dictionary<string, object> azure)
        {
            var issue = new UnifiedIssue
            {
                Id = azure.GetValueOrDefault("id", "").ToString() ?? "",
                Title = azure.GetValueOrDefault("title", "").ToString() ?? "",
                Description = azure.GetValueOrDefault("description", "").ToString() ?? "",
                Status = MapAzureStatus(azure.GetValueOrDefault("state", "").ToString() ?? ""),
                Priority = MapAzurePriority(azure.GetValueOrDefault("priority", "").ToString() ?? ""),
                Assignee = azure.GetValueOrDefault("assignedTo", "").ToString() ?? "",
                SourceSystem = SourceSystem.AzureDevOps,
                SourceId = azure.GetValueOrDefault("id", "").ToString() ?? "",
                SourceUrl = azure.GetValueOrDefault("url", "").ToString() ?? "",
                CreatedAt = ParseDateTime(azure.GetValueOrDefault("createdDate", DateTime.UtcNow)),
                UpdatedAt = ParseDateTime(azure.GetValueOrDefault("changedDate", DateTime.UtcNow)),
                LastSyncAt = DateTime.UtcNow
            };
            
            // Map tags as labels
            if (azure.TryGetValue("tags", out var tagsObj) && tagsObj is string tags)
            {
                issue.Labels = tags.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            
            // Map comments
            if (azure.TryGetValue("comments", out var commentsObj) && commentsObj is List<object> comments)
            {
                issue.Comments = comments.Select(c => MapAzureComment(c)).Where(c => c != null).ToList()!;
            }
            
            // Map attachments
            if (azure.TryGetValue("attachments", out var attachmentsObj) && attachmentsObj is List<object> attachments)
            {
                issue.Attachments = attachments.Select(a => MapAzureAttachment(a)).Where(a => a != null).ToList()!;
            }
            
            // Map custom fields
            if (azure.TryGetValue("customFields", out var customFieldsObj) && customFieldsObj is Dictionary<string, object> customFields)
            {
                issue.CustomFields = new Dictionary<string, object>(customFields);
            }
            
            return issue;
        }
        
        throw new ArgumentException("Invalid Azure DevOps work item format", nameof(azureWorkItem));
    }
    
    /// <summary>
    /// Compares this issue with another and returns a diff of changes
    /// </summary>
    public IssueDiff DiffWith(UnifiedIssue other)
    {
        var diff = new IssueDiff
        {
            IssueId = Id
        };
        
        // Compare basic fields
        CompareField(diff, nameof(Title), Title, other.Title, other.UpdatedAt, other.SourceSystem);
        CompareField(diff, nameof(Description), Description, other.Description, other.UpdatedAt, other.SourceSystem);
        CompareField(diff, nameof(Status), Status, other.Status, other.UpdatedAt, other.SourceSystem);
        CompareField(diff, nameof(Priority), Priority, other.Priority, other.UpdatedAt, other.SourceSystem);
        CompareField(diff, nameof(Assignee), Assignee, other.Assignee, other.UpdatedAt, other.SourceSystem);
        CompareField(diff, nameof(CompletedAt), CompletedAt, other.CompletedAt, other.UpdatedAt, other.SourceSystem);
        
        // Compare labels
        if (!Labels.SequenceEqual(other.Labels))
        {
            diff.FieldDifferences.Add(new FieldDifference
            {
                FieldName = nameof(Labels),
                OldValue = Labels.ToList(),
                NewValue = other.Labels.ToList(),
                LastModified = other.UpdatedAt,
                ModifiedBy = other.SourceSystem
            });
        }
        
        // Compare custom fields
        CompareCustomFields(diff, CustomFields, other.CustomFields, other.UpdatedAt, other.SourceSystem);
        
        // Compare comments
        CompareComments(diff, Comments, other.Comments);
        
        // Compare attachments
        CompareAttachments(diff, Attachments, other.Attachments);
        
        return diff;
    }
    
    private void CompareField<T>(IssueDiff diff, string fieldName, T oldValue, T newValue, DateTime lastModified, SourceSystem modifiedBy)
    {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            diff.FieldDifferences.Add(new FieldDifference
            {
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                LastModified = lastModified,
                ModifiedBy = modifiedBy
            });
        }
    }
    
    private void CompareCustomFields(IssueDiff diff, Dictionary<string, object> oldFields, Dictionary<string, object> newFields, DateTime lastModified, SourceSystem modifiedBy)
    {
        var allKeys = oldFields.Keys.Union(newFields.Keys);
        
        foreach (var key in allKeys)
        {
            var oldValue = oldFields.GetValueOrDefault(key);
            var newValue = newFields.GetValueOrDefault(key);
            
            if (!Equals(oldValue, newValue))
            {
                diff.FieldDifferences.Add(new FieldDifference
                {
                    FieldName = $"CustomFields.{key}",
                    OldValue = oldValue,
                    NewValue = newValue,
                    LastModified = lastModified,
                    ModifiedBy = modifiedBy
                });
            }
        }
    }
    
    private void CompareComments(IssueDiff diff, List<IssueComment> oldComments, List<IssueComment> newComments)
    {
        var oldCommentsById = oldComments.ToDictionary(c => c.Id);
        var newCommentsById = newComments.ToDictionary(c => c.Id);
        
        // Find added comments
        diff.AddedComments = newComments.Where(c => !oldCommentsById.ContainsKey(c.Id)).ToList();
        
        // Find removed comments
        diff.RemovedComments = oldComments.Where(c => !newCommentsById.ContainsKey(c.Id)).ToList();
        
        // Find modified comments
        diff.ModifiedComments = newComments.Where(newComment => 
            oldCommentsById.TryGetValue(newComment.Id, out var oldComment) && 
            !newComment.Equals(oldComment)).ToList();
    }
    
    private void CompareAttachments(IssueDiff diff, List<IssueAttachment> oldAttachments, List<IssueAttachment> newAttachments)
    {
        var oldAttachmentsById = oldAttachments.ToDictionary(a => a.Id);
        var newAttachmentsById = newAttachments.ToDictionary(a => a.Id);
        
        // Find added attachments
        diff.AddedAttachments = newAttachments.Where(a => !oldAttachmentsById.ContainsKey(a.Id)).ToList();
        
        // Find removed attachments
        diff.RemovedAttachments = oldAttachments.Where(a => !newAttachmentsById.ContainsKey(a.Id)).ToList();
    }
    
    // Helper methods for mapping source system data
    private static IssueStatus MapJiraStatus(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "new" or "open" or "to do" => IssueStatus.New,
            "in progress" or "in-progress" => IssueStatus.InProgress,
            "in review" or "review" => IssueStatus.InReview,
            "done" or "resolved" => IssueStatus.Done,
            "closed" => IssueStatus.Closed,
            "cancelled" or "canceled" => IssueStatus.Cancelled,
            _ => IssueStatus.New
        };
    }
    
    private static IssuePriority MapJiraPriority(string priority)
    {
        return priority.ToLowerInvariant() switch
        {
            "critical" or "highest" => IssuePriority.Critical,
            "high" or "major" => IssuePriority.High,
            "medium" or "normal" => IssuePriority.Medium,
            "low" or "minor" or "lowest" => IssuePriority.Low,
            _ => IssuePriority.Medium
        };
    }
    
    private static IssueStatus MapGitHubStatus(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "open" => IssueStatus.New,
            "closed" => IssueStatus.Done,
            _ => IssueStatus.New
        };
    }
    
    private static IssueStatus MapAzureStatus(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "new" => IssueStatus.New,
            "active" or "approved" => IssueStatus.InProgress,
            "resolved" => IssueStatus.Done,
            "closed" => IssueStatus.Closed,
            "removed" => IssueStatus.Cancelled,
            _ => IssueStatus.New
        };
    }
    
    private static IssuePriority MapAzurePriority(string priority)
    {
        return priority.ToLowerInvariant() switch
        {
            "1" or "critical" => IssuePriority.Critical,
            "2" or "high" => IssuePriority.High,
            "3" or "medium" => IssuePriority.Medium,
            "4" or "low" => IssuePriority.Low,
            _ => IssuePriority.Medium
        };
    }
    
    private static string ExtractGitHubAssignee(object? assignee)
    {
        if (assignee is Dictionary<string, object> assigneeDict)
        {
            var login = assigneeDict.GetValueOrDefault("login", "")?.ToString() ?? "";
            return string.IsNullOrWhiteSpace(login) ? "" : login.Trim();
        }
        return "";
    }
    
    private static string ExtractGitHubLabelName(object label)
    {
        if (label is Dictionary<string, object> labelDict)
        {
            return labelDict.GetValueOrDefault("name", "").ToString() ?? "";
        }
        return label.ToString() ?? "";
    }
    
    private static IssueComment? MapJiraComment(object comment)
    {
        if (comment is Dictionary<string, object> commentDict)
        {
            return new IssueComment
            {
                Id = commentDict.GetValueOrDefault("id", "").ToString() ?? "",
                Author = commentDict.GetValueOrDefault("author", "").ToString() ?? "",
                Body = commentDict.GetValueOrDefault("body", "").ToString() ?? "",
                CreatedAt = ParseDateTime(commentDict.GetValueOrDefault("created", DateTime.UtcNow)),
                UpdatedAt = ParseDateTime(commentDict.GetValueOrDefault("updated", DateTime.UtcNow))
            };
        }
        return null;
    }
    
    private static IssueComment? MapGitHubComment(object comment)
    {
        if (comment is Dictionary<string, object> commentDict)
        {
            return new IssueComment
            {
                Id = commentDict.GetValueOrDefault("id", "").ToString() ?? "",
                Author = ExtractGitHubCommentAuthor(commentDict.GetValueOrDefault("user", null)),
                Body = commentDict.GetValueOrDefault("body", "").ToString() ?? "",
                CreatedAt = ParseDateTime(commentDict.GetValueOrDefault("created_at", DateTime.UtcNow)),
                UpdatedAt = ParseDateTime(commentDict.GetValueOrDefault("updated_at", DateTime.UtcNow))
            };
        }
        return null;
    }
    
    private static IssueComment? MapAzureComment(object comment)
    {
        if (comment is Dictionary<string, object> commentDict)
        {
            return new IssueComment
            {
                Id = commentDict.GetValueOrDefault("id", "").ToString() ?? "",
                Author = commentDict.GetValueOrDefault("createdBy", "").ToString() ?? "",
                Body = commentDict.GetValueOrDefault("text", "").ToString() ?? "",
                CreatedAt = ParseDateTime(commentDict.GetValueOrDefault("createdDate", DateTime.UtcNow)),
                UpdatedAt = ParseDateTime(commentDict.GetValueOrDefault("modifiedDate", DateTime.UtcNow))
            };
        }
        return null;
    }
    
    private static string ExtractGitHubCommentAuthor(object? user)
    {
        if (user is Dictionary<string, object> userDict)
        {
            return userDict.GetValueOrDefault("login", "").ToString() ?? "";
        }
        return "";
    }
    
    private static IssueAttachment? MapJiraAttachment(object attachment)
    {
        if (attachment is Dictionary<string, object> attachmentDict)
        {
            return new IssueAttachment
            {
                Id = attachmentDict.GetValueOrDefault("id", "").ToString() ?? "",
                FileName = attachmentDict.GetValueOrDefault("filename", "").ToString() ?? "",
                Url = attachmentDict.GetValueOrDefault("content", "").ToString() ?? "",
                Size = Convert.ToInt64(attachmentDict.GetValueOrDefault("size", 0L)),
                ContentType = attachmentDict.GetValueOrDefault("mimeType", "").ToString() ?? "",
                UploadedAt = ParseDateTime(attachmentDict.GetValueOrDefault("created", DateTime.UtcNow))
            };
        }
        return null;
    }
    
    private static IssueAttachment? MapAzureAttachment(object attachment)
    {
        if (attachment is Dictionary<string, object> attachmentDict)
        {
            return new IssueAttachment
            {
                Id = attachmentDict.GetValueOrDefault("id", "").ToString() ?? "",
                FileName = attachmentDict.GetValueOrDefault("name", "").ToString() ?? "",
                Url = attachmentDict.GetValueOrDefault("url", "").ToString() ?? "",
                Size = Convert.ToInt64(attachmentDict.GetValueOrDefault("size", 0L)),
                ContentType = attachmentDict.GetValueOrDefault("contentType", "").ToString() ?? "",
                UploadedAt = ParseDateTime(attachmentDict.GetValueOrDefault("uploadedDate", DateTime.UtcNow))
            };
        }
        return null;
    }
    
    private static DateTime ParseDateTime(object dateObj)
    {
        if (dateObj is DateTime dt)
            return dt;
        if (dateObj is string dateStr && DateTime.TryParse(dateStr, out var parsed))
            return parsed;
        return DateTime.UtcNow;
    }
}