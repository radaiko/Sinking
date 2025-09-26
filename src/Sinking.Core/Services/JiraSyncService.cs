using Microsoft.Extensions.Logging;
using Sinking.Core.Configurations;
using Sinking.Core.Interfaces;
using Sinking.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Sinking.Core.Services;

public class JiraSyncService : ISyncService
{
    private readonly ILogger<JiraSyncService> _logger;
    private readonly HttpClient _httpClient;

    public JiraSyncService(ILogger<JiraSyncService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public SyncServiceType ServiceType => SyncServiceType.Jira;

    public async Task<IEnumerable<SyncableItem>> GetItemsAsync(string configuration)
    {
        var config = JsonSerializer.Deserialize<JiraConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid Jira configuration");

        ConfigureHttpClient(config);

        var jql = BuildJqlQuery(config);
        var searchUrl = $"{config.BaseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100&expand=renderedFields";

        var response = await _httpClient.GetAsync(searchUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<JiraSearchResult>(content);

        return searchResult?.Issues?.Select(MapToSyncableItem) ?? Array.Empty<SyncableItem>();
    }

    public async Task<SyncableItem?> GetItemAsync(string configuration, string itemId)
    {
        var config = JsonSerializer.Deserialize<JiraConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid Jira configuration");

        ConfigureHttpClient(config);

        var url = $"{config.BaseUrl}/rest/api/3/issue/{itemId}?expand=renderedFields";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var issue = JsonSerializer.Deserialize<JiraIssue>(content);

            return issue != null ? MapToSyncableItem(issue) : null;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null;
        }
    }

    public async Task<SyncableItem> CreateItemAsync(string configuration, SyncableItem item)
    {
        var config = JsonSerializer.Deserialize<JiraConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid Jira configuration");

        ConfigureHttpClient(config);

        var createRequest = new
        {
            fields = new
            {
                project = new { key = config.ProjectKey },
                summary = item.Title,
                description = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new { type = "text", text = item.Description ?? "" }
                            }
                        }
                    }
                },
                issuetype = new { name = config.IssueTypes?.FirstOrDefault() ?? "Story" },
                labels = item.Labels
            }
        };

        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{config.BaseUrl}/rest/api/3/issue", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<JiraCreateResult>(responseContent);

        // Fetch the created issue to get complete data
        return await GetItemAsync(configuration, createResult?.Key ?? "") ?? 
               throw new Exception("Failed to retrieve created issue");
    }

    public async Task<SyncableItem> UpdateItemAsync(string configuration, string itemId, SyncableItem item)
    {
        var config = JsonSerializer.Deserialize<JiraConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid Jira configuration");

        ConfigureHttpClient(config);

        var updateRequest = new
        {
            fields = new
            {
                summary = item.Title,
                description = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new { type = "text", text = item.Description ?? "" }
                            }
                        }
                    }
                },
                labels = item.Labels
            }
        };

        var json = JsonSerializer.Serialize(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"{config.BaseUrl}/rest/api/3/issue/{itemId}", content);
        response.EnsureSuccessStatusCode();

        return await GetItemAsync(configuration, itemId) ?? 
               throw new Exception("Failed to retrieve updated issue");
    }

    public async Task<IEnumerable<SyncableAttachment>> GetAttachmentsAsync(string configuration, string itemId)
    {
        var config = JsonSerializer.Deserialize<JiraConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid Jira configuration");

        ConfigureHttpClient(config);

        var response = await _httpClient.GetAsync($"{config.BaseUrl}/rest/api/3/issue/{itemId}?fields=attachment");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var issue = JsonSerializer.Deserialize<JiraIssue>(content);

        return issue?.Fields?.Attachment?.Select(MapToSyncableAttachment) ?? Array.Empty<SyncableAttachment>();
    }

    public async Task<SyncableAttachment> AddAttachmentAsync(string configuration, string itemId, SyncableAttachment attachment)
    {
        var config = JsonSerializer.Deserialize<JiraConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid Jira configuration");

        ConfigureHttpClient(config);

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(attachment.Content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(attachment.ContentType);
        form.Add(fileContent, "file", attachment.FileName);

        var response = await _httpClient.PostAsync($"{config.BaseUrl}/rest/api/3/issue/{itemId}/attachments", form);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var attachments = JsonSerializer.Deserialize<JiraAttachment[]>(responseContent);

        var jiraAttachment = attachments?.FirstOrDefault();
        if (jiraAttachment == null)
            throw new Exception("Failed to upload attachment");

        return MapToSyncableAttachment(jiraAttachment);
    }

    public async Task<string> ResolveUserAsync(string configuration, string userIdentifier)
    {
        var config = JsonSerializer.Deserialize<JiraConfiguration>(configuration);
        if (config == null) throw new ArgumentException("Invalid Jira configuration");

        ConfigureHttpClient(config);

        try
        {
            var searchUrl = $"{config.BaseUrl}/rest/api/3/user/search?query={Uri.EscapeDataString(userIdentifier)}";
            var response = await _httpClient.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<JiraUser[]>(content);

            return users?.FirstOrDefault()?.AccountId ?? userIdentifier;
        }
        catch
        {
            return userIdentifier;
        }
    }

    private void ConfigureHttpClient(JiraConfiguration config)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Email}:{config.ApiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static string BuildJqlQuery(JiraConfiguration config)
    {
        var jql = $"project = {config.ProjectKey}";

        if (config.IssueTypes?.Any() == true)
        {
            jql += $" AND issueType IN ({string.Join(",", config.IssueTypes.Select(t => $"\"{t}\""))})";
        }

        if (!string.IsNullOrEmpty(config.JqlFilter))
        {
            jql += $" AND ({config.JqlFilter})";
        }

        jql += " ORDER BY updated DESC";

        return jql;
    }

    private static SyncableItem MapToSyncableItem(JiraIssue issue)
    {
        return new SyncableItem
        {
            Id = issue.Key,
            Title = issue.Fields.Summary,
            Description = issue.Fields.Description?.Content?.FirstOrDefault()?.Content?.FirstOrDefault()?.Text,
            Status = issue.Fields.Status.Name,
            Assignee = issue.Fields.Assignee?.AccountId,
            Reporter = issue.Fields.Reporter?.AccountId,
            Labels = issue.Fields.Labels ?? new List<string>(),
            CreatedAt = DateTime.Parse(issue.Fields.Created),
            UpdatedAt = DateTime.Parse(issue.Fields.Updated),
            CustomFields = new Dictionary<string, object>
            {
                ["priority"] = issue.Fields.Priority?.Name ?? "",
                ["issueType"] = issue.Fields.IssueType.Name
            }
        };
    }

    private static SyncableAttachment MapToSyncableAttachment(JiraAttachment attachment)
    {
        return new SyncableAttachment
        {
            Id = attachment.Id,
            FileName = attachment.Filename,
            ContentType = attachment.MimeType,
            Size = attachment.Size,
            DownloadUrl = attachment.Content
        };
    }
}

// Jira API response models
public class JiraSearchResult
{
    public List<JiraIssue> Issues { get; set; } = new();
}

public class JiraIssue
{
    public string Key { get; set; } = "";
    public JiraIssueFields Fields { get; set; } = new();
}

public class JiraIssueFields
{
    public string Summary { get; set; } = "";
    public JiraDescription? Description { get; set; }
    public JiraStatus Status { get; set; } = new();
    public JiraUser? Assignee { get; set; }
    public JiraUser? Reporter { get; set; }
    public JiraIssueType IssueType { get; set; } = new();
    public JiraPriority? Priority { get; set; }
    public List<string> Labels { get; set; } = new();
    public string Created { get; set; } = "";
    public string Updated { get; set; } = "";
    public List<JiraAttachment>? Attachment { get; set; }
}

public class JiraDescription
{
    public List<JiraContent> Content { get; set; } = new();
}

public class JiraContent
{
    public List<JiraContentItem> Content { get; set; } = new();
}

public class JiraContentItem
{
    public string Text { get; set; } = "";
}

public class JiraStatus
{
    public string Name { get; set; } = "";
}

public class JiraUser
{
    public string AccountId { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

public class JiraIssueType
{
    public string Name { get; set; } = "";
}

public class JiraPriority
{
    public string Name { get; set; } = "";
}

public class JiraAttachment
{
    public string Id { get; set; } = "";
    public string Filename { get; set; } = "";
    public string MimeType { get; set; } = "";
    public long Size { get; set; }
    public string Content { get; set; } = "";
}

public class JiraCreateResult
{
    public string Key { get; set; } = "";
}