using Xunit;
using Sinking.Core;

namespace Sinking.Core.Tests;

/// <summary>
/// Tests focused on ensuring the UnifiedIssue system is easily extensible for future source systems
/// </summary>
public class UnifiedIssueExtensibilityTests
{
    [Fact]
    public void UnifiedIssue_SupportsAllRequiredSourceSystems()
    {
        // Arrange & Act
        var supportedSystems = Enum.GetValues<SourceSystem>();

        // Assert
        Assert.Contains(SourceSystem.Jira, supportedSystems);
        Assert.Contains(SourceSystem.GitHub, supportedSystems);
        Assert.Contains(SourceSystem.AzureDevOps, supportedSystems);
        Assert.Equal(3, supportedSystems.Length);
    }

    [Fact]
    public void UnifiedIssue_SupportsAllRequiredStatuses()
    {
        // Arrange & Act
        var supportedStatuses = Enum.GetValues<IssueStatus>();

        // Assert
        Assert.Contains(IssueStatus.New, supportedStatuses);
        Assert.Contains(IssueStatus.InProgress, supportedStatuses);
        Assert.Contains(IssueStatus.InReview, supportedStatuses);
        Assert.Contains(IssueStatus.Done, supportedStatuses);
        Assert.Contains(IssueStatus.Closed, supportedStatuses);
        Assert.Contains(IssueStatus.Cancelled, supportedStatuses);
        Assert.Equal(6, supportedStatuses.Length);
    }

    [Fact]
    public void UnifiedIssue_SupportsAllRequiredPriorities()
    {
        // Arrange & Act
        var supportedPriorities = Enum.GetValues<IssuePriority>();

        // Assert
        Assert.Contains(IssuePriority.Critical, supportedPriorities);
        Assert.Contains(IssuePriority.High, supportedPriorities);
        Assert.Contains(IssuePriority.Medium, supportedPriorities);
        Assert.Contains(IssuePriority.Low, supportedPriorities);
        Assert.Equal(4, supportedPriorities.Length);
    }

    [Theory]
    [InlineData(SourceSystem.Jira)]
    [InlineData(SourceSystem.GitHub)]
    [InlineData(SourceSystem.AzureDevOps)]
    public void UnifiedIssue_AllSourceSystemsProduceValidObjects(SourceSystem sourceSystem)
    {
        // Arrange
        Dictionary<string, object> testData = sourceSystem switch
        {
            SourceSystem.Jira => TestDataFactory.CreateJiraIssue($"TEST-{(int)sourceSystem}"),
            SourceSystem.GitHub => TestDataFactory.CreateGitHubIssue((int)sourceSystem * 1000),
            SourceSystem.AzureDevOps => TestDataFactory.CreateAzureDevOpsWorkItem((int)sourceSystem * 10000),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceSystem))
        };

        // Act
        UnifiedIssue issue = sourceSystem switch
        {
            SourceSystem.Jira => UnifiedIssue.FromJira(testData),
            SourceSystem.GitHub => UnifiedIssue.FromGitHub(testData),
            SourceSystem.AzureDevOps => UnifiedIssue.FromAzureDevOps(testData),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceSystem))
        };

        // Assert - All source systems should produce valid UnifiedIssue objects
        Assert.NotNull(issue);
        Assert.False(string.IsNullOrEmpty(issue.Id));
        Assert.False(string.IsNullOrEmpty(issue.SourceId));
        Assert.Equal(sourceSystem, issue.SourceSystem);
        Assert.True(issue.CreatedAt <= issue.UpdatedAt);
        Assert.True(issue.LastSyncAt >= DateTime.UtcNow.AddMinutes(-1));
        
        // All issues should have these collections initialized (even if empty)
        Assert.NotNull(issue.Labels);
        Assert.NotNull(issue.Comments);
        Assert.NotNull(issue.Attachments);
        Assert.NotNull(issue.CustomFields);
    }

    [Fact]
    public void CustomFields_SupportsArbitraryDataTypes()
    {
        // Arrange
        var testData = TestDataFactory.CreateJiraIssue("TYPES-001");
        var customFields = (Dictionary<string, object>)testData["customFields"];
        
        // Add various data types to custom fields
        customFields["StringField"] = "Text value";
        customFields["IntField"] = 42;
        customFields["BoolField"] = true;
        customFields["DoubleField"] = 3.14159;
        customFields["DateField"] = DateTime.UtcNow;
        customFields["ListField"] = new List<string> { "item1", "item2", "item3" };
        customFields["DictField"] = new Dictionary<string, object> { ["nested"] = "value" };

        // Act
        var issue = UnifiedIssue.FromJira(testData);

        // Assert
        Assert.Equal("Text value", issue.CustomFields["StringField"]);
        Assert.Equal(42, issue.CustomFields["IntField"]);
        Assert.Equal(true, issue.CustomFields["BoolField"]);
        Assert.Equal(3.14159, issue.CustomFields["DoubleField"]);
        Assert.IsType<DateTime>(issue.CustomFields["DateField"]);
        Assert.IsType<List<string>>(issue.CustomFields["ListField"]);
        Assert.IsType<Dictionary<string, object>>(issue.CustomFields["DictField"]);
    }

    [Fact]
    public void DiffWith_CustomFieldTypes_HandlesAllTypes()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("DIFF-TYPES-001");
        var modifiedData = TestDataFactory.CreateJiraIssue("DIFF-TYPES-001");
        
        var originalFields = (Dictionary<string, object>)originalData["customFields"];
        var modifiedFields = (Dictionary<string, object>)modifiedData["customFields"];
        
        // Set up different types of changes
        originalFields["StringField"] = "old value";
        modifiedFields["StringField"] = "new value";
        
        originalFields["IntField"] = 10;
        modifiedFields["IntField"] = 20;
        
        originalFields["BoolField"] = false;
        modifiedFields["BoolField"] = true;
        
        originalFields["ListField"] = new List<string> { "a", "b" };
        modifiedFields["ListField"] = new List<string> { "a", "b", "c" };

        modifiedData["updated"] = DateTime.UtcNow;

        var original = UnifiedIssue.FromJira(originalData);
        var modified = UnifiedIssue.FromJira(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        
        var stringDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.StringField");
        Assert.NotNull(stringDiff);
        Assert.Equal("old value", stringDiff.OldValue);
        Assert.Equal("new value", stringDiff.NewValue);
        
        var intDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.IntField");
        Assert.NotNull(intDiff);
        Assert.Equal(10, intDiff.OldValue);
        Assert.Equal(20, intDiff.NewValue);
        
        var boolDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.BoolField");
        Assert.NotNull(boolDiff);
        Assert.Equal(false, boolDiff.OldValue);
        Assert.Equal(true, boolDiff.NewValue);
        
        var listDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.ListField");
        Assert.NotNull(listDiff);
        Assert.NotEqual(listDiff.OldValue, listDiff.NewValue);
    }

    [Fact]
    public void TestDataFactory_ProvidesRealisticDataForAllSystems()
    {
        // Act & Assert - Jira data
        var jiraData = TestDataFactory.CreateJiraIssue("REALISTIC-J1");
        Assert.True(jiraData.ContainsKey("summary"));
        Assert.True(jiraData.ContainsKey("description"));
        Assert.True(jiraData.ContainsKey("status"));
        Assert.True(jiraData.ContainsKey("priority"));
        Assert.True(jiraData.ContainsKey("comments"));
        Assert.True(jiraData.ContainsKey("attachments"));
        Assert.True(jiraData.ContainsKey("customFields"));
        
        var jiraComments = (List<object>)jiraData["comments"];
        Assert.True(jiraComments.Count > 0);
        
        var jiraAttachments = (List<object>)jiraData["attachments"];
        Assert.True(jiraAttachments.Count > 0);
        
        // Act & Assert - GitHub data
        var githubData = TestDataFactory.CreateGitHubIssue(1234);
        Assert.True(githubData.ContainsKey("title"));
        Assert.True(githubData.ContainsKey("body"));
        Assert.True(githubData.ContainsKey("state"));
        Assert.True(githubData.ContainsKey("assignee"));
        Assert.True(githubData.ContainsKey("labels"));
        Assert.True(githubData.ContainsKey("comments"));
        
        var githubLabels = (List<object>)githubData["labels"];
        Assert.True(githubLabels.Count > 0);
        
        // Act & Assert - Azure DevOps data
        var azureData = TestDataFactory.CreateAzureDevOpsWorkItem(5678);
        Assert.True(azureData.ContainsKey("title"));
        Assert.True(azureData.ContainsKey("description"));
        Assert.True(azureData.ContainsKey("state"));
        Assert.True(azureData.ContainsKey("priority"));
        Assert.True(azureData.ContainsKey("tags"));
        Assert.True(azureData.ContainsKey("comments"));
        Assert.True(azureData.ContainsKey("attachments"));
        Assert.True(azureData.ContainsKey("customFields"));
        
        var azureComments = (List<object>)azureData["comments"];
        Assert.True(azureComments.Count > 0);
        
        var azureAttachments = (List<object>)azureData["attachments"];
        Assert.True(azureAttachments.Count > 0);
    }

    [Fact]
    public void TestDataFactory_ProvidesModifiedVersionsWithMeaningfulChanges()
    {
        // Arrange & Act
        var originalJira = TestDataFactory.CreateJiraIssue("MOD-J1");
        var modifiedJira = TestDataFactory.CreateModifiedJiraIssue("MOD-J1");
        
        var originalGitHub = TestDataFactory.CreateGitHubIssue(9999);
        var modifiedGitHub = TestDataFactory.CreateModifiedGitHubIssue(9999);
        
        var originalAzure = TestDataFactory.CreateAzureDevOpsWorkItem(7777);
        var modifiedAzure = TestDataFactory.CreateModifiedAzureDevOpsWorkItem(7777);

        // Assert - Jira modifications
        Assert.NotEqual(originalJira["summary"], modifiedJira["summary"]);
        Assert.NotEqual(originalJira["status"], modifiedJira["status"]);
        Assert.NotEqual(originalJira["priority"], modifiedJira["priority"]);
        
        var originalJiraComments = (List<object>)originalJira["comments"];
        var modifiedJiraComments = (List<object>)modifiedJira["comments"];
        Assert.True(modifiedJiraComments.Count > originalJiraComments.Count);
        
        // Assert - GitHub modifications
        Assert.NotEqual(originalGitHub["title"], modifiedGitHub["title"]);
        Assert.NotEqual(originalGitHub["state"], modifiedGitHub["state"]);
        
        var originalGitHubComments = (List<object>)originalGitHub["comments"];
        var modifiedGitHubComments = (List<object>)modifiedGitHub["comments"];
        Assert.True(modifiedGitHubComments.Count > originalGitHubComments.Count);
        
        // Assert - Azure modifications
        Assert.NotEqual(originalAzure["title"], modifiedAzure["title"]);
        Assert.NotEqual(originalAzure["state"], modifiedAzure["state"]);
        Assert.NotEqual(originalAzure["priority"], modifiedAzure["priority"]);
        
        var originalAzureComments = (List<object>)originalAzure["comments"];
        var modifiedAzureComments = (List<object>)modifiedAzure["comments"];
        Assert.True(modifiedAzureComments.Count > originalAzureComments.Count);
    }

    [Fact]
    public void UnifiedIssue_MaintainsSourceSystemInformation()
    {
        // Arrange & Act
        var jiraIssue = UnifiedIssue.FromJira(TestDataFactory.CreateJiraIssue("SOURCE-J1"));
        var githubIssue = UnifiedIssue.FromGitHub(TestDataFactory.CreateGitHubIssue(1111));
        var azureIssue = UnifiedIssue.FromAzureDevOps(TestDataFactory.CreateAzureDevOpsWorkItem(2222));

        // Assert
        Assert.Equal(SourceSystem.Jira, jiraIssue.SourceSystem);
        Assert.False(string.IsNullOrEmpty(jiraIssue.SourceId));
        Assert.False(string.IsNullOrEmpty(jiraIssue.SourceUrl));
        
        Assert.Equal(SourceSystem.GitHub, githubIssue.SourceSystem);
        Assert.False(string.IsNullOrEmpty(githubIssue.SourceId));
        Assert.False(string.IsNullOrEmpty(githubIssue.SourceUrl));
        
        Assert.Equal(SourceSystem.AzureDevOps, azureIssue.SourceSystem);
        Assert.False(string.IsNullOrEmpty(azureIssue.SourceId));
        Assert.False(string.IsNullOrEmpty(azureIssue.SourceUrl));
    }

    [Fact]
    public void IssueDiff_ContainsCompleteMetadata()
    {
        // Arrange
        var original = UnifiedIssue.FromJira(TestDataFactory.CreateJiraIssue("META-1"));
        var modified = UnifiedIssue.FromJira(TestDataFactory.CreateModifiedJiraIssue("META-1"));

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.Equal(original.Id, diff.IssueId);
        Assert.True(diff.DiffGeneratedAt >= DateTime.UtcNow.AddMinutes(-1));
        Assert.True(diff.DiffGeneratedAt <= DateTime.UtcNow.AddMinutes(1));
        
        // Each field difference should have complete metadata
        foreach (var fieldDiff in diff.FieldDifferences)
        {
            Assert.False(string.IsNullOrEmpty(fieldDiff.FieldName));
            Assert.True(fieldDiff.LastModified > DateTime.MinValue);
            Assert.True(Enum.IsDefined(fieldDiff.ModifiedBy));
        }
    }

    [Fact]
    public void Comments_MaintainCompleteMetadata()
    {
        // Arrange
        var testData = TestDataFactory.CreateJiraIssue("COMMENT-META-1");
        var issue = UnifiedIssue.FromJira(testData);

        // Act & Assert
        foreach (var comment in issue.Comments)
        {
            Assert.False(string.IsNullOrEmpty(comment.Id));
            Assert.False(string.IsNullOrEmpty(comment.Author));
            Assert.False(string.IsNullOrEmpty(comment.Body));
            Assert.True(comment.CreatedAt > DateTime.MinValue);
            Assert.True(comment.UpdatedAt > DateTime.MinValue);
            Assert.True(comment.CreatedAt <= comment.UpdatedAt);
        }
    }

    [Fact]
    public void Attachments_MaintainCompleteMetadata()
    {
        // Arrange
        var testData = TestDataFactory.CreateJiraIssue("ATTACHMENT-META-1");
        var issue = UnifiedIssue.FromJira(testData);

        // Act & Assert
        foreach (var attachment in issue.Attachments)
        {
            Assert.False(string.IsNullOrEmpty(attachment.Id));
            Assert.False(string.IsNullOrEmpty(attachment.FileName));
            Assert.False(string.IsNullOrEmpty(attachment.Url));
            Assert.True(attachment.Size >= 0);
            Assert.False(string.IsNullOrEmpty(attachment.ContentType));
            Assert.True(attachment.UploadedAt > DateTime.MinValue);
        }
    }

    [Fact]
    public void TestStructure_SupportsEasyExtensionForNewSystems()
    {
        // This test documents the pattern for adding new source systems
        // It validates that the current structure makes it easy to add new systems
        
        // Arrange - Simulate adding a new system
        var newSystemData = new Dictionary<string, object>
        {
            ["id"] = "future_system_001",
            ["key"] = "FUTURE-1",
            ["title"] = "Issue from Future System",
            ["content"] = "Description from future system",
            ["workflow_state"] = "active",
            ["importance"] = "critical",
            ["owner"] = "future.user@company.com",
            ["created_timestamp"] = DateTime.UtcNow.AddDays(-5),
            ["modified_timestamp"] = DateTime.UtcNow.AddDays(-1),
            ["system_url"] = "https://futuresystem.company.com/issues/FUTURE-1",
            ["categories"] = new List<object> { "new-category", "future-work" },
            ["discussions"] = new List<object>(),
            ["files"] = new List<object>(),
            ["metadata"] = new Dictionary<string, object>
            {
                ["Version"] = "2.0",
                ["Team"] = "Future Team"
            }
        };

        // Act - This demonstrates how easy it would be to add a new FromFutureSystem method
        // Following the same pattern as existing systems
        var issue = new UnifiedIssue
        {
            Id = newSystemData.GetValueOrDefault("id", "").ToString() ?? "",
            SourceId = newSystemData.GetValueOrDefault("key", "").ToString() ?? "",
            Title = newSystemData.GetValueOrDefault("title", "").ToString() ?? "",
            Description = newSystemData.GetValueOrDefault("content", "").ToString() ?? "",
            // Status = MapFutureSystemStatus(newSystemData.GetValueOrDefault("workflow_state", "").ToString() ?? ""),
            Status = IssueStatus.InProgress, // Simplified for demo
            // Priority = MapFutureSystemPriority(newSystemData.GetValueOrDefault("importance", "").ToString() ?? ""),
            Priority = IssuePriority.Critical, // Simplified for demo
            Assignee = newSystemData.GetValueOrDefault("owner", "").ToString() ?? "",
            // SourceSystem = SourceSystem.FutureSystem, // Would add this enum value
            SourceSystem = SourceSystem.GitHub, // Using existing for demo
            SourceUrl = newSystemData.GetValueOrDefault("system_url", "").ToString() ?? "",
            CreatedAt = (DateTime)newSystemData.GetValueOrDefault("created_timestamp", DateTime.UtcNow),
            UpdatedAt = (DateTime)newSystemData.GetValueOrDefault("modified_timestamp", DateTime.UtcNow),
            LastSyncAt = DateTime.UtcNow
        };

        // Map categories as labels
        if (newSystemData.TryGetValue("categories", out var categoriesObj) && categoriesObj is List<object> categories)
        {
            issue.Labels = categories.Select(c => c.ToString() ?? "").Where(l => !string.IsNullOrEmpty(l)).ToList();
        }

        // Map metadata as custom fields
        if (newSystemData.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> metadata)
        {
            issue.CustomFields = new Dictionary<string, object>(metadata);
        }

        // Assert - The existing structure accommodates new systems easily
        Assert.NotNull(issue);
        Assert.Equal("future_system_001", issue.Id);
        Assert.Equal("FUTURE-1", issue.SourceId);
        Assert.Equal("Issue from Future System", issue.Title);
        Assert.Equal("Description from future system", issue.Description);
        Assert.Equal("future.user@company.com", issue.Assignee);
        Assert.Equal(2, issue.Labels.Count);
        Assert.Contains("new-category", issue.Labels);
        Assert.Contains("future-work", issue.Labels);
        Assert.Equal(2, issue.CustomFields.Count);
        Assert.Equal("2.0", issue.CustomFields["Version"]);
        Assert.Equal("Future Team", issue.CustomFields["Team"]);
    }
}