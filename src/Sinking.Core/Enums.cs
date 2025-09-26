namespace Sinking.Core;

/// <summary>
/// Represents the status of a unified issue across different systems
/// </summary>
public enum IssueStatus
{
    New,
    InProgress,
    InReview,
    Done,
    Closed,
    Cancelled
}

/// <summary>
/// Represents the priority level of an issue
/// </summary>
public enum IssuePriority
{
    Critical,
    High,
    Medium,
    Low
}

/// <summary>
/// Represents the source system that an issue originated from
/// </summary>
public enum SourceSystem
{
    Jira,
    GitHub,
    AzureDevOps
}