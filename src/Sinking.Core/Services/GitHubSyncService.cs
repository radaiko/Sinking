using Microsoft.Extensions.Logging;
using Octokit;
using Sinking.Core.Configurations;
using Sinking.Core.Interfaces;
using Sinking.Core.Models;
using System.Text.Json;

namespace Sinking.Core.Services;

public class GitHubSyncService : ISyncService
{
    private readonly ILogger<GitHubSyncService> _logger;
    private readonly HttpClient _httpClient;

    public GitHubSyncService(ILogger<GitHubSyncService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public SyncServiceType ServiceType => SyncServiceType.GitHub;

    public async Task<IEnumerable<SyncableItem>> GetItemsAsync(string configuration)
    {
        var config = JsonSerializer.Deserialize<GitHubConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid GitHub configuration");

        var client = CreateClient(config);
        
        var issueRequest = new RepositoryIssueRequest
        {
            State = config.IncludeClosedIssues ? ItemStateFilter.All : ItemStateFilter.Open
        };

        if (config.Labels?.Any() == true)
        {
            foreach (var label in config.Labels)
            {
                issueRequest.Labels.Add(label);
            }
        }

        if (!string.IsNullOrEmpty(config.Milestone))
        {
            issueRequest.Milestone = config.Milestone;
        }

        var issues = await client.Issue.GetAllForRepository(config.Owner, config.Repository, issueRequest);
        
        return issues.Select(MapToSyncableItem);
    }

    public async Task<SyncableItem?> GetItemAsync(string configuration, string itemId)
    {
        var config = JsonSerializer.Deserialize<GitHubConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid GitHub configuration");

        var client = CreateClient(config);
        
        try
        {
            var issue = await client.Issue.Get(config.Owner, config.Repository, int.Parse(itemId));
            return MapToSyncableItem(issue);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<SyncableItem> CreateItemAsync(string configuration, SyncableItem item)
    {
        var config = JsonSerializer.Deserialize<GitHubConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid GitHub configuration");

        var client = CreateClient(config);
        
        var createIssue = new NewIssue(item.Title)
        {
            Body = item.Description
        };

        foreach (var label in item.Labels)
        {
            createIssue.Labels.Add(label);
        }

        if (!string.IsNullOrEmpty(item.Assignee))
        {
            createIssue.Assignees.Add(item.Assignee);
        }

        var issue = await client.Issue.Create(config.Owner, config.Repository, createIssue);
        return MapToSyncableItem(issue);
    }

    public async Task<SyncableItem> UpdateItemAsync(string configuration, string itemId, SyncableItem item)
    {
        var config = JsonSerializer.Deserialize<GitHubConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid GitHub configuration");

        var client = CreateClient(config);
        
        var updateIssue = new IssueUpdate
        {
            Title = item.Title,
            Body = item.Description,
            State = item.Status.ToLowerInvariant() == "closed" ? ItemState.Closed : ItemState.Open
        };

        updateIssue.Labels.Clear();
        foreach (var label in item.Labels)
        {
            updateIssue.Labels.Add(label);
        }

        var issue = await client.Issue.Update(config.Owner, config.Repository, int.Parse(itemId), updateIssue);
        return MapToSyncableItem(issue);
    }

    public async Task<IEnumerable<SyncableAttachment>> GetAttachmentsAsync(string configuration, string itemId)
    {
        // GitHub doesn't have native attachments like Jira, but we can parse attachments from comments or body
        var item = await GetItemAsync(configuration, itemId);
        if (item == null) return Array.Empty<SyncableAttachment>();

        var attachments = new List<SyncableAttachment>();
        
        // Parse attachments from issue body and comments
        // This is a simplified implementation - in practice, you'd parse markdown for image/file links
        
        return attachments;
    }

    public async Task<SyncableAttachment> AddAttachmentAsync(string configuration, string itemId, SyncableAttachment attachment)
    {
        // GitHub doesn't support direct file attachments via API
        // In practice, you'd upload to a storage service and add a link in a comment
        throw new NotSupportedException("GitHub API doesn't support direct file attachments. Consider uploading to external storage and adding links.");
    }

    public async Task<string> ResolveUserAsync(string configuration, string userIdentifier)
    {
        var config = JsonSerializer.Deserialize<GitHubConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid GitHub configuration");

        var client = CreateClient(config);
        
        try
        {
            // Try to get user by username
            var user = await client.User.Get(userIdentifier);
            return user.Login;
        }
        catch (NotFoundException)
        {
            // If not found, return the original identifier
            return userIdentifier;
        }
    }

    private GitHubClient CreateClient(GitHubConfiguration config)
    {
        var client = new GitHubClient(new ProductHeaderValue("Sinking-Sync"));
        
        if (!string.IsNullOrEmpty(config.AccessToken))
        {
            client.Credentials = new Credentials(config.AccessToken);
        }
        
        return client;
    }

    private static SyncableItem MapToSyncableItem(Issue issue)
    {
        return new SyncableItem
        {
            Id = issue.Number.ToString(),
            Title = issue.Title,
            Description = issue.Body,
            Status = issue.State.StringValue,
            Assignee = issue.Assignee?.Login,
            Reporter = issue.User.Login,
            Labels = issue.Labels.Select(l => l.Name).ToList(),
            CreatedAt = issue.CreatedAt.DateTime,
            UpdatedAt = issue.UpdatedAt?.DateTime ?? issue.CreatedAt.DateTime,
            CustomFields = new Dictionary<string, object>
            {
                ["html_url"] = issue.HtmlUrl,
                ["milestone"] = issue.Milestone?.Title ?? string.Empty,
                ["state"] = issue.State.StringValue
            }
        };
    }
}