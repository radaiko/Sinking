namespace Sinking.Core;

/// <summary>
/// Handles mapping and extraction of data from different source systems
/// </summary>
internal static class SourceSystemMappers
{
    /// <summary>
    /// Maps Jira status strings to unified issue status
    /// </summary>
    public static IssueStatus MapJiraStatus(string status)
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
    
    /// <summary>
    /// Maps Jira priority strings to unified issue priority
    /// </summary>
    public static IssuePriority MapJiraPriority(string priority)
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
    
    /// <summary>
    /// Maps GitHub state strings to unified issue status
    /// </summary>
    public static IssueStatus MapGitHubStatus(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "open" => IssueStatus.New,
            "closed" => IssueStatus.Done,
            _ => IssueStatus.New
        };
    }
    
    /// <summary>
    /// Maps Azure DevOps state strings to unified issue status
    /// </summary>
    public static IssueStatus MapAzureStatus(string state)
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
    
    /// <summary>
    /// Maps Azure DevOps priority strings to unified issue priority
    /// </summary>
    public static IssuePriority MapAzurePriority(string priority)
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
    
    /// <summary>
    /// Extracts GitHub assignee login from user object
    /// </summary>
    public static string ExtractGitHubAssignee(object? assignee)
    {
        if (assignee is Dictionary<string, object> assigneeDict)
        {
            var login = assigneeDict.GetValueOrDefault(FieldNames.GitHub.Login, "")?.ToString() ?? "";
            return string.IsNullOrWhiteSpace(login) ? "" : login.Trim();
        }
        return "";
    }
    
    /// <summary>
    /// Extracts GitHub label name from label object
    /// </summary>
    public static string ExtractGitHubLabelName(object label)
    {
        if (label is Dictionary<string, object> labelDict)
        {
            return labelDict.GetValueOrDefault(FieldNames.GitHub.Name, "")?.ToString() ?? "";
        }
        return label.ToString() ?? "";
    }
    
    /// <summary>
    /// Extracts GitHub comment author from user object
    /// </summary>
    public static string ExtractGitHubCommentAuthor(object? user)
    {
        if (user is Dictionary<string, object> userDict)
        {
            return userDict.GetValueOrDefault(FieldNames.GitHub.Login, "")?.ToString() ?? "";
        }
        return "";
    }
    
    /// <summary>
    /// Parses date/time object to DateTime
    /// </summary>
    public static DateTime ParseDateTime(object dateObj)
    {
        if (dateObj is DateTime dt)
            return dt;
        if (dateObj is string dateStr && DateTime.TryParse(dateStr, out var parsed))
            return parsed;
        return DateTime.UtcNow;
    }
}