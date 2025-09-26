namespace Sinking.Core.Configurations;

public class GitHubConfiguration
{
    public string? AccessToken { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public List<string>? Labels { get; set; }
    public string? Milestone { get; set; }
    public bool IncludeClosedIssues { get; set; } = false;
}

public class JiraConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public List<string>? IssueTypes { get; set; }
    public string? JqlFilter { get; set; }
}

public class AzureDevOpsConfiguration
{
    public string OrganizationUrl { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string PersonalAccessToken { get; set; } = string.Empty;
    public string WorkItemType { get; set; } = "User Story";
    public List<string>? AreaPaths { get; set; }
    public List<string>? IterationPaths { get; set; }
}