using Xunit;
using Sinking.Core;

namespace Sinking.Core.Tests;

public class UnifiedIssueTests
{
    [Fact]
    public void FromJira_WithValidData_CreatesUnifiedIssueCorrectly()
    {
        // Arrange
        var jiraData = TestDataFactory.CreateJiraIssue("PROJ-123");

        // Act
        var issue = UnifiedIssue.FromJira(jiraData);

        // Assert
        Assert.Equal("10001", issue.Id);
        Assert.Equal("PROJ-123", issue.SourceId);
        Assert.Equal("Implement user authentication system", issue.Title);
        Assert.Equal("We need to implement a robust user authentication system that supports OAuth 2.0, SAML, and traditional username/password authentication. This should include proper session management and security features.", issue.Description);
        Assert.Equal(IssueStatus.InProgress, issue.Status);
        Assert.Equal(IssuePriority.High, issue.Priority);
        Assert.Equal("john.doe@company.com", issue.Assignee);
        Assert.Equal(SourceSystem.Jira, issue.SourceSystem);
        Assert.Equal("https://company.atlassian.net/browse/PROJ-123", issue.SourceUrl);
        
        // Verify labels
        Assert.Equal(3, issue.Labels.Count);
        Assert.Contains("authentication", issue.Labels);
        Assert.Contains("security", issue.Labels);
        Assert.Contains("backend", issue.Labels);
        
        // Verify comments
        Assert.Equal(2, issue.Comments.Count);
        Assert.Equal("comment_001", issue.Comments[0].Id);
        Assert.Equal("jane.smith@company.com", issue.Comments[0].Author);
        Assert.Contains("OAuth 2.0", issue.Comments[0].Body);
        
        // Verify attachments
        Assert.Single(issue.Attachments);
        Assert.Equal("attach_001", issue.Attachments[0].Id);
        Assert.Equal("auth_flow_diagram.png", issue.Attachments[0].FileName);
        Assert.Equal(245760L, issue.Attachments[0].Size);
        Assert.Equal("image/png", issue.Attachments[0].ContentType);
        
        // Verify custom fields
        Assert.Equal(5, issue.CustomFields.Count);
        Assert.Equal(8, issue.CustomFields["Story Points"]);
        Assert.Equal("PROJ-100", issue.CustomFields["Epic Link"]);
        Assert.Equal("High", issue.CustomFields["Business Value"]);
        
        // Verify timestamps
        Assert.True(issue.CreatedAt <= issue.UpdatedAt);
        Assert.True(issue.LastSyncAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void FromGitHub_WithValidData_CreatesUnifiedIssueCorrectly()
    {
        // Arrange
        var githubData = TestDataFactory.CreateGitHubIssue(456);

        // Act
        var issue = UnifiedIssue.FromGitHub(githubData);

        // Assert
        Assert.Equal("987654321", issue.Id);
        Assert.Equal("456", issue.SourceId);
        Assert.Equal("Add REST API documentation", issue.Title);
        Assert.Contains("comprehensive REST API documentation", issue.Description);
        Assert.Equal(IssueStatus.New, issue.Status);
        Assert.Equal(IssuePriority.Medium, issue.Priority); // Default for GitHub
        Assert.Equal("api-docs-team", issue.Assignee);
        Assert.Equal(SourceSystem.GitHub, issue.SourceSystem);
        Assert.Equal("https://github.com/company/project/issues/456", issue.SourceUrl);
        
        // Verify labels
        Assert.Equal(3, issue.Labels.Count);
        Assert.Contains("documentation", issue.Labels);
        Assert.Contains("api", issue.Labels);
        Assert.Contains("high-priority", issue.Labels);
        
        // Verify comments
        Assert.Single(issue.Comments);
        Assert.Equal("gh_comment_001", issue.Comments[0].Id);
        Assert.Equal("developer1", issue.Comments[0].Author);
        Assert.Contains("OpenAPI specification", issue.Comments[0].Body);
        
        // Verify custom fields
        Assert.Equal(3, issue.CustomFields.Count);
        Assert.Equal("v2.0", issue.CustomFields["Milestone"]);
        Assert.Equal(16, issue.CustomFields["Estimated Hours"]);
        
        // Verify timestamps
        Assert.True(issue.LastSyncAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void FromAzureDevOps_WithValidData_CreatesUnifiedIssueCorrectly()
    {
        // Arrange
        var azureData = TestDataFactory.CreateAzureDevOpsWorkItem(789);

        // Act
        var issue = UnifiedIssue.FromAzureDevOps(azureData);

        // Assert
        Assert.Equal("789", issue.Id);
        Assert.Equal("789", issue.SourceId);
        Assert.Equal("Database performance optimization", issue.Title);
        Assert.Contains("Optimize database queries", issue.Description);
        Assert.Equal(IssueStatus.InProgress, issue.Status);
        Assert.Equal(IssuePriority.High, issue.Priority);
        Assert.Equal("database.admin@company.com", issue.Assignee);
        Assert.Equal(SourceSystem.AzureDevOps, issue.SourceSystem);
        Assert.Equal("https://dev.azure.com/company/project/_workitems/edit/789", issue.SourceUrl);
        
        // Verify labels (from tags)
        Assert.Equal(4, issue.Labels.Count);
        Assert.Contains("performance", issue.Labels);
        Assert.Contains("database", issue.Labels);
        Assert.Contains("optimization", issue.Labels);
        Assert.Contains("production", issue.Labels);
        
        // Verify comments
        Assert.Single(issue.Comments);
        Assert.Equal("azure_comment_001", issue.Comments[0].Id);
        Assert.Equal("performance.engineer@company.com", issue.Comments[0].Author);
        Assert.Contains("missing indexes", issue.Comments[0].Body);
        
        // Verify attachments
        Assert.Single(issue.Attachments);
        Assert.Equal("azure_attach_001", issue.Attachments[0].Id);
        Assert.Equal("query_performance_report.pdf", issue.Attachments[0].FileName);
        Assert.Equal("application/pdf", issue.Attachments[0].ContentType);
        
        // Verify custom fields
        Assert.Equal(5, issue.CustomFields.Count);
        Assert.Equal(20, issue.CustomFields["Effort"]);
        Assert.Equal("High", issue.CustomFields["Business Impact"]);
        
        // Verify timestamps
        Assert.True(issue.LastSyncAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Theory]
    [InlineData("new", IssueStatus.New)]
    [InlineData("open", IssueStatus.New)]
    [InlineData("to do", IssueStatus.New)]
    [InlineData("in progress", IssueStatus.InProgress)]
    [InlineData("in-progress", IssueStatus.InProgress)]
    [InlineData("in review", IssueStatus.InReview)]
    [InlineData("review", IssueStatus.InReview)]
    [InlineData("done", IssueStatus.Done)]
    [InlineData("resolved", IssueStatus.Done)]
    [InlineData("closed", IssueStatus.Closed)]
    [InlineData("cancelled", IssueStatus.Cancelled)]
    [InlineData("unknown", IssueStatus.New)]
    public void FromJira_StatusMapping_MapsCorrectly(string jiraStatus, IssueStatus expectedStatus)
    {
        // Arrange
        var jiraData = TestDataFactory.CreateJiraIssue();
        jiraData["status"] = jiraStatus;

        // Act
        var issue = UnifiedIssue.FromJira(jiraData);

        // Assert
        Assert.Equal(expectedStatus, issue.Status);
    }

    [Theory]
    [InlineData("critical", IssuePriority.Critical)]
    [InlineData("highest", IssuePriority.Critical)]
    [InlineData("high", IssuePriority.High)]
    [InlineData("major", IssuePriority.High)]
    [InlineData("medium", IssuePriority.Medium)]
    [InlineData("normal", IssuePriority.Medium)]
    [InlineData("low", IssuePriority.Low)]
    [InlineData("minor", IssuePriority.Low)]
    [InlineData("lowest", IssuePriority.Low)]
    [InlineData("unknown", IssuePriority.Medium)]
    public void FromJira_PriorityMapping_MapsCorrectly(string jiraPriority, IssuePriority expectedPriority)
    {
        // Arrange
        var jiraData = TestDataFactory.CreateJiraIssue();
        jiraData["priority"] = jiraPriority;

        // Act
        var issue = UnifiedIssue.FromJira(jiraData);

        // Assert
        Assert.Equal(expectedPriority, issue.Priority);
    }

    [Theory]
    [InlineData("open", IssueStatus.New)]
    [InlineData("closed", IssueStatus.Done)]
    [InlineData("unknown", IssueStatus.New)]
    public void FromGitHub_StatusMapping_MapsCorrectly(string githubState, IssueStatus expectedStatus)
    {
        // Arrange
        var githubData = TestDataFactory.CreateGitHubIssue();
        githubData["state"] = githubState;

        // Act
        var issue = UnifiedIssue.FromGitHub(githubData);

        // Assert
        Assert.Equal(expectedStatus, issue.Status);
    }

    [Theory]
    [InlineData("new", IssueStatus.New)]
    [InlineData("active", IssueStatus.InProgress)]
    [InlineData("approved", IssueStatus.InProgress)]
    [InlineData("resolved", IssueStatus.Done)]
    [InlineData("closed", IssueStatus.Closed)]
    [InlineData("removed", IssueStatus.Cancelled)]
    [InlineData("unknown", IssueStatus.New)]
    public void FromAzureDevOps_StatusMapping_MapsCorrectly(string azureState, IssueStatus expectedStatus)
    {
        // Arrange
        var azureData = TestDataFactory.CreateAzureDevOpsWorkItem();
        azureData["state"] = azureState;

        // Act
        var issue = UnifiedIssue.FromAzureDevOps(azureData);

        // Assert
        Assert.Equal(expectedStatus, issue.Status);
    }

    [Theory]
    [InlineData("1", IssuePriority.Critical)]
    [InlineData("critical", IssuePriority.Critical)]
    [InlineData("2", IssuePriority.High)]
    [InlineData("high", IssuePriority.High)]
    [InlineData("3", IssuePriority.Medium)]
    [InlineData("medium", IssuePriority.Medium)]
    [InlineData("4", IssuePriority.Low)]
    [InlineData("low", IssuePriority.Low)]
    [InlineData("unknown", IssuePriority.Medium)]
    public void FromAzureDevOps_PriorityMapping_MapsCorrectly(string azurePriority, IssuePriority expectedPriority)
    {
        // Arrange
        var azureData = TestDataFactory.CreateAzureDevOpsWorkItem();
        azureData["priority"] = azurePriority;

        // Act
        var issue = UnifiedIssue.FromAzureDevOps(azureData);

        // Assert
        Assert.Equal(expectedPriority, issue.Priority);
    }

    [Fact]
    public void DiffWith_NoChanges_ReturnsEmptyDiff()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("PROJ-123");
        var issue1 = UnifiedIssue.FromJira(originalData);
        var issue2 = UnifiedIssue.FromJira(originalData);

        // Act
        var diff = issue1.DiffWith(issue2);

        // Assert
        Assert.False(diff.HasChanges);
        Assert.Empty(diff.FieldDifferences);
        Assert.Empty(diff.AddedComments);
        Assert.Empty(diff.ModifiedComments);
        Assert.Empty(diff.RemovedComments);
        Assert.Empty(diff.AddedAttachments);
        Assert.Empty(diff.RemovedAttachments);
    }

    [Fact]
    public void DiffWith_BasicFieldChanges_ReturnsCorrectDiff()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("PROJ-123");
        var modifiedData = TestDataFactory.CreateModifiedJiraIssue("PROJ-123");
        
        var original = UnifiedIssue.FromJira(originalData);
        var modified = UnifiedIssue.FromJira(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        Assert.Equal("10001", diff.IssueId);
        
        // Check field differences
        var titleDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Title");
        Assert.NotNull(titleDiff);
        Assert.Equal("Implement user authentication system", titleDiff.OldValue);
        Assert.Equal("Implement user authentication system with 2FA", titleDiff.NewValue);
        Assert.Equal(SourceSystem.Jira, titleDiff.ModifiedBy);

        var statusDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Status");
        Assert.NotNull(statusDiff);
        Assert.Equal(IssueStatus.InProgress, statusDiff.OldValue);
        Assert.Equal(IssueStatus.InReview, statusDiff.NewValue);

        var priorityDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Priority");
        Assert.NotNull(priorityDiff);
        Assert.Equal(IssuePriority.High, priorityDiff.OldValue);
        Assert.Equal(IssuePriority.Critical, priorityDiff.NewValue);

        var assigneeDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Assignee");
        Assert.NotNull(assigneeDiff);
        Assert.Equal("john.doe@company.com", assigneeDiff.OldValue);
        Assert.Equal("jane.smith@company.com", assigneeDiff.NewValue);
        
        // Check that added comments are detected
        Assert.Single(diff.AddedComments);
        Assert.Equal("comment_003", diff.AddedComments[0].Id);
        Assert.Contains("two-factor authentication", diff.AddedComments[0].Body);
    }

    [Fact]
    public void DiffWith_CustomFieldChanges_ReturnsCorrectDiff()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("PROJ-123");
        var modifiedData = TestDataFactory.CreateModifiedJiraIssue("PROJ-123");
        
        var original = UnifiedIssue.FromJira(originalData);
        var modified = UnifiedIssue.FromJira(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        var storyPointsDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.Story Points");
        Assert.NotNull(storyPointsDiff);
        Assert.Equal(8, storyPointsDiff.OldValue);
        Assert.Equal(13, storyPointsDiff.NewValue);

        var complexityDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.Technical Complexity");
        Assert.NotNull(complexityDiff);
        Assert.Equal("Medium", complexityDiff.OldValue);
        Assert.Equal("High", complexityDiff.NewValue);

        var newFieldDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.Security Review Required");
        Assert.NotNull(newFieldDiff);
        Assert.Null(newFieldDiff.OldValue);
        Assert.Equal(true, newFieldDiff.NewValue);
    }

    [Fact]
    public void DiffWith_CommentChanges_ReturnsCorrectDiff()
    {
        // Arrange
        var originalData = TestDataFactory.CreateGitHubIssue(456);
        var modifiedData = TestDataFactory.CreateModifiedGitHubIssue(456);
        
        var original = UnifiedIssue.FromGitHub(originalData);
        var modified = UnifiedIssue.FromGitHub(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.Single(diff.AddedComments);
        Assert.Equal("gh_comment_002", diff.AddedComments[0].Id);
        Assert.Equal("api-docs-team", diff.AddedComments[0].Author);
        Assert.Contains("Documentation is now complete", diff.AddedComments[0].Body);
    }

    [Fact]
    public void DiffWith_CrossSystemChanges_TracksSources()
    {
        // Arrange - Create same issue from different systems
        var jiraData = TestDataFactory.CreateJiraIssue("PROJ-123");
        var azureData = TestDataFactory.CreateModifiedAzureDevOpsWorkItem(789);
        
        // Simulate converting the same conceptual issue
        azureData["title"] = "Implement user authentication system - Updated";
        azureData["state"] = "Resolved";
        
        var jiraIssue = UnifiedIssue.FromJira(jiraData);
        var azureIssue = UnifiedIssue.FromAzureDevOps(azureData);
        azureIssue.Id = jiraIssue.Id; // Same conceptual issue

        // Act
        var diff = jiraIssue.DiffWith(azureIssue);

        // Assert
        Assert.True(diff.HasChanges);
        
        // Verify that changes are attributed to Azure DevOps
        var titleDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Title");
        Assert.NotNull(titleDiff);
        Assert.Equal(SourceSystem.AzureDevOps, titleDiff.ModifiedBy);
        
        var statusDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Status");
        Assert.NotNull(statusDiff);
        Assert.Equal(SourceSystem.AzureDevOps, statusDiff.ModifiedBy);
    }

    [Fact]
    public void FromJira_WithEmptyData_HandlesGracefully()
    {
        // Arrange
        var emptyData = TestDataFactory.CreateEmptyJiraIssue("EMPTY-1");

        // Act
        var issue = UnifiedIssue.FromJira(emptyData);

        // Assert
        Assert.Equal("99999", issue.Id);
        Assert.Equal("EMPTY-1", issue.SourceId);
        Assert.Equal("", issue.Title);
        Assert.Equal("", issue.Description);
        Assert.Equal(IssueStatus.New, issue.Status); // Default mapping for empty status
        Assert.Equal(IssuePriority.Medium, issue.Priority); // Default mapping for empty priority
        Assert.Equal("", issue.Assignee);
        Assert.Empty(issue.Labels);
        Assert.Empty(issue.Comments);
        Assert.Empty(issue.Attachments);
        Assert.Empty(issue.CustomFields);
    }

    [Fact]
    public void FromJira_WithMissingFields_UsesDefaults()
    {
        // Arrange
        var minimalData = new Dictionary<string, object>
        {
            ["id"] = "minimal_001",
            ["key"] = "MIN-1",
            ["summary"] = "Minimal issue"
        };

        // Act
        var issue = UnifiedIssue.FromJira(minimalData);

        // Assert
        Assert.Equal("minimal_001", issue.Id);
        Assert.Equal("MIN-1", issue.SourceId);
        Assert.Equal("Minimal issue", issue.Title);
        Assert.Equal("", issue.Description);
        Assert.Equal(IssueStatus.New, issue.Status);
        Assert.Equal(IssuePriority.Medium, issue.Priority);
        Assert.Equal("", issue.Assignee);
        Assert.Empty(issue.Labels);
        Assert.Empty(issue.Comments);
        Assert.Empty(issue.Attachments);
        Assert.Empty(issue.CustomFields);
        Assert.True(issue.LastSyncAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void FromJira_WithInvalidData_ThrowsException()
    {
        // Arrange
        var invalidData = "not a dictionary";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => UnifiedIssue.FromJira(invalidData));
    }

    [Fact]
    public void FromGitHub_WithInvalidData_ThrowsException()
    {
        // Arrange
        var invalidData = new List<string> { "not", "a", "dictionary" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => UnifiedIssue.FromGitHub(invalidData));
    }

    [Fact]
    public void FromAzureDevOps_WithInvalidData_ThrowsException()
    {
        // Arrange
        var invalidData = 12345;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => UnifiedIssue.FromAzureDevOps(invalidData));
    }

    [Fact]
    public void DiffWith_AttachmentChanges_DetectsCorrectly()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("PROJ-123");
        var original = UnifiedIssue.FromJira(originalData);
        
        var modifiedData = TestDataFactory.CreateJiraIssue("PROJ-123");
        var attachments = (List<object>)modifiedData["attachments"];
        
        // Remove existing attachment and add a new one
        attachments.Clear();
        attachments.Add(new Dictionary<string, object>
        {
            ["id"] = "attach_002",
            ["filename"] = "updated_diagram.png",
            ["content"] = "https://company.atlassian.net/secure/attachment/10002/updated_diagram.png",
            ["size"] = 512000L,
            ["mimeType"] = "image/png",
            ["created"] = DateTime.UtcNow
        });
        
        var modified = UnifiedIssue.FromJira(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        Assert.Single(diff.AddedAttachments);
        Assert.Single(diff.RemovedAttachments);
        
        Assert.Equal("attach_002", diff.AddedAttachments[0].Id);
        Assert.Equal("updated_diagram.png", diff.AddedAttachments[0].FileName);
        
        Assert.Equal("attach_001", diff.RemovedAttachments[0].Id);
        Assert.Equal("auth_flow_diagram.png", diff.RemovedAttachments[0].FileName);
    }

    [Fact]
    public void DiffWith_LabelChanges_DetectsCorrectly()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("PROJ-123");
        var original = UnifiedIssue.FromJira(originalData);
        
        var modifiedData = TestDataFactory.CreateJiraIssue("PROJ-123");
        modifiedData["labels"] = new List<object> { "authentication", "security", "frontend", "urgent" }; // Changed backend->frontend, added urgent
        modifiedData["updated"] = DateTime.UtcNow;
        
        var modified = UnifiedIssue.FromJira(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        
        var labelsDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Labels");
        Assert.NotNull(labelsDiff);
        
        var oldLabels = (List<string>)labelsDiff.OldValue!;
        var newLabels = (List<string>)labelsDiff.NewValue!;
        
        Assert.Equal(3, oldLabels.Count);
        Assert.Contains("backend", oldLabels);
        Assert.DoesNotContain("frontend", oldLabels);
        Assert.DoesNotContain("urgent", oldLabels);
        
        Assert.Equal(4, newLabels.Count);
        Assert.DoesNotContain("backend", newLabels);
        Assert.Contains("frontend", newLabels);
        Assert.Contains("urgent", newLabels);
    }
}