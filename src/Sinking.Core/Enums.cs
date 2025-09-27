namespace Sinking.Core;

/// <summary>
/// Represents the status of a unified issue across different systems
/// </summary>
public enum IssueStatus
{
    /// <summary>
    /// Issue is newly created and not yet started
    /// </summary>
    New,
    
    /// <summary>
    /// Issue is currently being worked on
    /// </summary>
    InProgress,
    
    /// <summary>
    /// Issue is completed but under review
    /// </summary>
    InReview,
    
    /// <summary>
    /// Issue is completed and ready for deployment
    /// </summary>
    Done,
    
    /// <summary>
    /// Issue is completed and closed
    /// </summary>
    Closed,
    
    /// <summary>
    /// Issue was cancelled and will not be completed
    /// </summary>
    Cancelled
}

/// <summary>
/// Represents the priority level of an issue
/// </summary>
public enum IssuePriority
{
    /// <summary>
    /// Highest priority - requires immediate attention
    /// </summary>
    Critical,
    
    /// <summary>
    /// High priority - should be addressed soon
    /// </summary>
    High,
    
    /// <summary>
    /// Medium priority - normal priority level
    /// </summary>
    Medium,
    
    /// <summary>
    /// Low priority - can be addressed when time permits
    /// </summary>
    Low
}

/// <summary>
/// Represents the source system that an issue originated from
/// </summary>
public enum SourceSystem
{
    /// <summary>
    /// Atlassian Jira issue tracking system
    /// </summary>
    Jira,
    
    /// <summary>
    /// GitHub issue tracking system
    /// </summary>
    GitHub,
    
    /// <summary>
    /// Microsoft Azure DevOps work item tracking system
    /// </summary>
    AzureDevOps
}