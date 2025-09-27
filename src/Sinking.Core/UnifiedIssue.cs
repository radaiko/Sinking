namespace Sinking.Core;

/// <summary>
/// Unified representation of an issue from different source systems (Jira, GitHub, Azure DevOps)
/// </summary>
public class UnifiedIssue
{
    /// <summary>
    /// Gets or sets the unique identifier for this issue
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the title or summary of the issue
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the detailed description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the current status of the issue
    /// </summary>
    public IssueStatus Status { get; set; } = IssueStatus.New;
    
    /// <summary>
    /// Gets or sets the priority level of the issue
    /// </summary>
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    
    /// <summary>
    /// Gets or sets the person assigned to work on this issue
    /// </summary>
    public string Assignee { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the list of labels or tags associated with this issue
    /// </summary>
    public List<string> Labels { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the timestamp when the issue was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the issue was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the issue was completed (if applicable)
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the list of comments on this issue
    /// </summary>
    public List<IssueComment> Comments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of attachments on this issue
    /// </summary>
    public List<IssueAttachment> Attachments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the dictionary of custom fields specific to the source system
    /// </summary>
    public Dictionary<string, object> CustomFields { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the source system where this issue originated
    /// </summary>
    public SourceSystem SourceSystem { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier in the source system
    /// </summary>
    public string SourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URL to view this issue in the source system
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when this issue was last synchronized
    /// </summary>
    public DateTime LastSyncAt { get; set; }
    
    /// <summary>
    /// Creates a UnifiedIssue from a Jira work item
    /// </summary>
    /// <param name="jiraIssue">The Jira issue data as a dictionary</param>
    /// <returns>A unified issue representation</returns>
    /// <exception cref="ArgumentException">Thrown when the input data is not in the expected format</exception>
    public static UnifiedIssue FromJira(object jiraIssue)
    {
        if (jiraIssue is not Dictionary<string, object> jira)
        {
            throw new ArgumentException("Invalid Jira issue format", nameof(jiraIssue));
        }
        
        var issue = CreateBaseIssue(jira, SourceSystem.Jira);
        
        // Set Jira-specific properties
        issue.Title = jira.GetValueOrDefault(FieldNames.Jira.Summary, "")?.ToString() ?? "";
        issue.Status = SourceSystemMappers.MapJiraStatus(jira.GetValueOrDefault(FieldNames.Status, "")?.ToString() ?? "");
        issue.Priority = SourceSystemMappers.MapJiraPriority(jira.GetValueOrDefault(FieldNames.Priority, "")?.ToString() ?? "");
        issue.SourceId = jira.GetValueOrDefault(FieldNames.Jira.Key, "")?.ToString() ?? "";
        issue.CreatedAt = SourceSystemMappers.ParseDateTime(jira.GetValueOrDefault(FieldNames.Jira.Created, DateTime.UtcNow));
        issue.UpdatedAt = SourceSystemMappers.ParseDateTime(jira.GetValueOrDefault(FieldNames.Jira.Updated, DateTime.UtcNow));
        
        // Map collections
        MapJiraCollections(jira, issue);
        
        return issue;
    }
    
    /// <summary>
    /// Creates a UnifiedIssue from a GitHub issue
    /// </summary>
    /// <param name="githubIssue">The GitHub issue data as a dictionary</param>
    /// <returns>A unified issue representation</returns>
    /// <exception cref="ArgumentException">Thrown when the input data is not in the expected format</exception>
    public static UnifiedIssue FromGitHub(object githubIssue)
    {
        if (githubIssue is not Dictionary<string, object> github)
        {
            throw new ArgumentException("Invalid GitHub issue format", nameof(githubIssue));
        }

        var issue = CreateBaseIssue(github, SourceSystem.GitHub);
        
        // Set GitHub-specific properties
        issue.Title = github.GetValueOrDefault(FieldNames.Title, "")?.ToString() ?? "";
        issue.Description = github.GetValueOrDefault(FieldNames.GitHub.Body, "")?.ToString() ?? "";
        issue.Status = SourceSystemMappers.MapGitHubStatus(github.GetValueOrDefault(FieldNames.GitHub.State, "")?.ToString() ?? "");
        issue.Priority = IssuePriority.Medium; // GitHub doesn't have built-in priority
        issue.Assignee = SourceSystemMappers.ExtractGitHubAssignee(github.GetValueOrDefault(FieldNames.Assignee, null!));
        issue.SourceId = github.GetValueOrDefault(FieldNames.GitHub.Number, "")?.ToString() ?? "";
        issue.SourceUrl = github.GetValueOrDefault(FieldNames.GitHub.HtmlUrl, "")?.ToString() ?? "";
        issue.CreatedAt = SourceSystemMappers.ParseDateTime(github.GetValueOrDefault(FieldNames.CreatedAt, DateTime.UtcNow));
        issue.UpdatedAt = SourceSystemMappers.ParseDateTime(github.GetValueOrDefault(FieldNames.UpdatedAt, DateTime.UtcNow));
        
        // Map collections
        MapGitHubCollections(github, issue);
        
        return issue;
    }
    
    /// <summary>
    /// Creates a UnifiedIssue from an Azure DevOps work item
    /// </summary>
    /// <param name="azureWorkItem">The Azure DevOps work item data as a dictionary</param>
    /// <returns>A unified issue representation</returns>
    /// <exception cref="ArgumentException">Thrown when the input data is not in the expected format</exception>
    public static UnifiedIssue FromAzureDevOps(object azureWorkItem)
    {
        if (azureWorkItem is not Dictionary<string, object> azure)
        {
            throw new ArgumentException("Invalid Azure DevOps work item format", nameof(azureWorkItem));
        }

        var issue = CreateBaseIssue(azure, SourceSystem.AzureDevOps);
        
        // Set Azure-specific properties
        issue.Status = SourceSystemMappers.MapAzureStatus(azure.GetValueOrDefault(FieldNames.Azure.State, "")?.ToString() ?? "");
        issue.Priority = SourceSystemMappers.MapAzurePriority(azure.GetValueOrDefault(FieldNames.Priority, "")?.ToString() ?? "");
        issue.Assignee = azure.GetValueOrDefault(FieldNames.Azure.AssignedTo, "")?.ToString() ?? "";
        issue.CreatedAt = SourceSystemMappers.ParseDateTime(azure.GetValueOrDefault(FieldNames.Azure.CreatedDate, DateTime.UtcNow));
        issue.UpdatedAt = SourceSystemMappers.ParseDateTime(azure.GetValueOrDefault(FieldNames.Azure.ChangedDate, DateTime.UtcNow));
        
        // Map collections
        MapAzureCollections(azure, issue);
        
        return issue;
    }
    
    /// <summary>
    /// Compares this issue with another and returns a diff of changes
    /// </summary>
    /// <param name="other">The other UnifiedIssue to compare with</param>
    /// <returns>An IssueDiff object containing all detected changes</returns>
    /// <exception cref="ArgumentNullException">Thrown when other is null</exception>
    public IssueDiff DiffWith(UnifiedIssue other)
    {
        ArgumentNullException.ThrowIfNull(other);
        
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
    
    /// <summary>
    /// Compares a field value between two issues and adds a difference if they differ
    /// </summary>
    /// <typeparam name="T">The type of the field being compared</typeparam>
    /// <param name="diff">The diff object to add the difference to</param>
    /// <param name="fieldName">The name of the field being compared</param>
    /// <param name="oldValue">The old value of the field</param>
    /// <param name="newValue">The new value of the field</param>
    /// <param name="lastModified">The timestamp when the field was last modified</param>
    /// <param name="modifiedBy">The source system that made the modification</param>
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
    
    /// <summary>
    /// Compares custom fields between two issues and adds differences for any changes
    /// </summary>
    /// <param name="diff">The diff object to add differences to</param>
    /// <param name="oldFields">The old custom fields dictionary</param>
    /// <param name="newFields">The new custom fields dictionary</param>
    /// <param name="lastModified">The timestamp when the fields were last modified</param>
    /// <param name="modifiedBy">The source system that made the modification</param>
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
    
    /// <summary>
    /// Creates a base UnifiedIssue with common properties
    /// </summary>
    private static UnifiedIssue CreateBaseIssue(Dictionary<string, object> data, SourceSystem sourceSystem)
    {
        return new UnifiedIssue
        {
            Id = data.GetValueOrDefault(FieldNames.Id, "")?.ToString() ?? "",
            Title = data.GetValueOrDefault(FieldNames.Title, "")?.ToString() ?? "",
            Description = data.GetValueOrDefault(FieldNames.Description, "")?.ToString() ?? "",
            Assignee = data.GetValueOrDefault(FieldNames.Assignee, "")?.ToString() ?? "",
            SourceSystem = sourceSystem,
            SourceId = data.GetValueOrDefault(FieldNames.Id, "")?.ToString() ?? "",
            SourceUrl = data.GetValueOrDefault(FieldNames.Url, "")?.ToString() ?? "",
            LastSyncAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Maps Jira-specific collections (labels, comments, attachments, custom fields)
    /// </summary>
    private static void MapJiraCollections(Dictionary<string, object> jira, UnifiedIssue issue)
    {
        // Map labels
        if (jira.TryGetValue(FieldNames.Jira.Labels, out var labelsObj) && labelsObj is List<object> labels)
        {
            issue.Labels = labels.Select(l => l.ToString() ?? "").Where(l => !string.IsNullOrEmpty(l)).ToList();
        }
        
        // Map comments
        if (jira.TryGetValue(FieldNames.Jira.Comments, out var commentsObj) && commentsObj is List<object> comments)
        {
            issue.Comments = comments.Select(CommentAndAttachmentMappers.MapJiraComment)
                                  .Where(c => c != null)
                                  .ToList()!;
        }
        
        // Map attachments
        if (jira.TryGetValue(FieldNames.Jira.Attachments, out var attachmentsObj) && attachmentsObj is List<object> attachments)
        {
            issue.Attachments = attachments.Select(CommentAndAttachmentMappers.MapJiraAttachment)
                                         .Where(a => a != null)
                                         .ToList()!;
        }
        
        // Map custom fields
        if (jira.TryGetValue(FieldNames.Jira.CustomFields, out var customFieldsObj) && customFieldsObj is Dictionary<string, object> customFields)
        {
            issue.CustomFields = new Dictionary<string, object>(customFields);
        }
    }
    
    /// <summary>
    /// Maps GitHub-specific collections (labels, comments, custom fields)
    /// </summary>
    private static void MapGitHubCollections(Dictionary<string, object> github, UnifiedIssue issue)
    {
        // Map labels
        if (github.TryGetValue(FieldNames.GitHub.Labels, out var labelsObj) && labelsObj is List<object> labels)
        {
            issue.Labels = labels.Select(SourceSystemMappers.ExtractGitHubLabelName)
                                .Where(l => !string.IsNullOrEmpty(l))
                                .ToList();
        }
        
        // Map comments
        if (github.TryGetValue(FieldNames.GitHub.Comments, out var commentsObj) && commentsObj is List<object> comments)
        {
            issue.Comments = comments.Select(CommentAndAttachmentMappers.MapGitHubComment)
                                   .Where(c => c != null)
                                   .ToList()!;
        }
        
        // Map custom fields from metadata
        if (github.TryGetValue(FieldNames.Jira.CustomFields, out var customFieldsObj) && customFieldsObj is Dictionary<string, object> customFields)
        {
            issue.CustomFields = new Dictionary<string, object>(customFields);
        }
    }
    
    /// <summary>
    /// Maps Azure DevOps-specific collections (tags as labels, comments, attachments, custom fields)
    /// </summary>
    private static void MapAzureCollections(Dictionary<string, object> azure, UnifiedIssue issue)
    {
        // Map tags as labels
        if (azure.TryGetValue(FieldNames.Azure.Tags, out var tagsObj) && tagsObj is string tags)
        {
            issue.Labels = tags.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        
        // Map comments
        if (azure.TryGetValue(FieldNames.Azure.Comments, out var commentsObj) && commentsObj is List<object> comments)
        {
            issue.Comments = comments.Select(CommentAndAttachmentMappers.MapAzureComment)
                                   .Where(c => c != null)
                                   .ToList()!;
        }
        
        // Map attachments
        if (azure.TryGetValue(FieldNames.Azure.Attachments, out var attachmentsObj) && attachmentsObj is List<object> attachments)
        {
            issue.Attachments = attachments.Select(CommentAndAttachmentMappers.MapAzureAttachment)
                                         .Where(a => a != null)
                                         .ToList()!;
        }
        
        // Map custom fields
        if (azure.TryGetValue(FieldNames.Azure.CustomFields, out var customFieldsObj) && customFieldsObj is Dictionary<string, object> customFields)
        {
            issue.CustomFields = new Dictionary<string, object>(customFields);
        }
    }
}