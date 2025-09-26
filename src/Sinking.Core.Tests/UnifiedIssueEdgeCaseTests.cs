using Xunit;
using Sinking.Core;

namespace Sinking.Core.Tests;

/// <summary>
/// Additional tests covering edge cases and advanced scenarios
/// </summary>
public class UnifiedIssueEdgeCaseTests
{
    [Fact]
    public void DiffWith_IdenticalIssues_ReturnsNoDifferences()
    {
        // Arrange
        var data = TestDataFactory.CreateJiraIssue("EDGE-001");
        var issue1 = UnifiedIssue.FromJira(data);
        var issue2 = UnifiedIssue.FromJira(data);
        
        // Ensure same timestamps
        issue2.UpdatedAt = issue1.UpdatedAt;
        issue2.LastSyncAt = issue1.LastSyncAt;

        // Act
        var diff = issue1.DiffWith(issue2);

        // Assert
        Assert.False(diff.HasChanges);
        Assert.Empty(diff.FieldDifferences);
        Assert.Equal(DateTime.UtcNow.Date, diff.DiffGeneratedAt.Date);
    }

    [Fact]
    public void DiffWith_MultipleSimultaneousChanges_CapturesAll()
    {
        // Arrange
        var original = UnifiedIssue.FromJira(TestDataFactory.CreateJiraIssue("MULTI-001"));
        var modified = UnifiedIssue.FromJira(TestDataFactory.CreateJiraIssue("MULTI-001"));

        // Make multiple changes
        modified.Title = "Updated Title";
        modified.Description = "Updated Description";
        modified.Status = IssueStatus.Done;
        modified.Priority = IssuePriority.Critical;
        modified.Assignee = "new.assignee@company.com";
        modified.Labels = new List<string> { "updated", "labels" };
        modified.CustomFields["New Field"] = "New Value";
        modified.UpdatedAt = DateTime.UtcNow;

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        Assert.Equal(7, diff.FieldDifferences.Count); // Title, Description, Status, Priority, Assignee, Labels, CustomFields.New Field
        
        Assert.Contains(diff.FieldDifferences, d => d.FieldName == "Title");
        Assert.Contains(diff.FieldDifferences, d => d.FieldName == "Description");
        Assert.Contains(diff.FieldDifferences, d => d.FieldName == "Status");
        Assert.Contains(diff.FieldDifferences, d => d.FieldName == "Priority");
        Assert.Contains(diff.FieldDifferences, d => d.FieldName == "Assignee");
        Assert.Contains(diff.FieldDifferences, d => d.FieldName == "Labels");
        Assert.Contains(diff.FieldDifferences, d => d.FieldName == "CustomFields.New Field");
    }

    [Fact]
    public void FromJira_WithNullValues_HandlesGracefully()
    {
        // Arrange
        var dataWithNulls = new Dictionary<string, object>
        {
            ["id"] = "null_test_001",
            ["key"] = "NULL-1",
            ["summary"] = null!, // null title
            ["description"] = null!, // null description
            ["status"] = null!, // null status
            ["priority"] = null!, // null priority
            ["assignee"] = null!, // null assignee
            ["created"] = DateTime.UtcNow.AddDays(-1),
            ["updated"] = DateTime.UtcNow,
            ["url"] = "https://company.atlassian.net/browse/NULL-1",
            ["labels"] = null!, // null labels
            ["comments"] = null!, // null comments
            ["attachments"] = null!, // null attachments
            ["customFields"] = null! // null custom fields
        };

        // Act
        var issue = UnifiedIssue.FromJira(dataWithNulls);

        // Assert
        Assert.Equal("null_test_001", issue.Id);
        Assert.Equal("NULL-1", issue.SourceId);
        Assert.Equal("", issue.Title); // null becomes empty string
        Assert.Equal("", issue.Description);
        Assert.Equal(IssueStatus.New, issue.Status); // null maps to default
        Assert.Equal(IssuePriority.Medium, issue.Priority); // null maps to default
        Assert.Equal("", issue.Assignee);
        Assert.Empty(issue.Labels);
        Assert.Empty(issue.Comments);
        Assert.Empty(issue.Attachments);
        Assert.Empty(issue.CustomFields);
    }

    [Fact]
    public void FromJira_WithTypeMismatches_HandlesGracefully()
    {
        // Arrange - Data with wrong types that should be converted to strings
        var mismatchedData = new Dictionary<string, object>
        {
            ["id"] = 123456, // int instead of string
            ["key"] = 789, // int instead of string
            ["summary"] = 42, // int instead of string
            ["description"] = true, // bool instead of string
            ["status"] = 100, // int instead of string
            ["priority"] = false, // bool instead of string
            ["assignee"] = 3.14, // double instead of string
            ["created"] = DateTime.UtcNow.AddDays(-1),
            ["updated"] = DateTime.UtcNow,
            ["url"] = new Uri("https://company.atlassian.net/browse/MISMATCH-1")
        };

        // Act
        var issue = UnifiedIssue.FromJira(mismatchedData);

        // Assert
        Assert.Equal("123456", issue.Id);
        Assert.Equal("789", issue.SourceId);
        Assert.Equal("42", issue.Title);
        Assert.Equal("True", issue.Description);
        Assert.Equal(IssueStatus.New, issue.Status); // "100" doesn't match any known status
        Assert.Equal(IssuePriority.Medium, issue.Priority); // "False" doesn't match any known priority
        Assert.Equal("3.14", issue.Assignee);
    }

    [Fact]
    public void DiffWith_OnlyCommentsChanged_DetectsSpecificCommentChanges()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("COMMENT-001");
        var modifiedData = TestDataFactory.CreateJiraIssue("COMMENT-001");
        
        var comments = (List<object>)modifiedData["comments"];
        
        // Modify existing comment
        var existingComment = (Dictionary<string, object>)comments[0];
        existingComment["body"] = "Modified: " + existingComment["body"];
        existingComment["updated"] = DateTime.UtcNow;
        
        // Remove second comment
        comments.RemoveAt(1);
        
        // Add new comment
        comments.Add(new Dictionary<string, object>
        {
            ["id"] = "new_comment_001",
            ["author"] = "modifier@company.com",
            ["body"] = "This is a brand new comment added during modification.",
            ["created"] = DateTime.UtcNow,
            ["updated"] = DateTime.UtcNow
        });

        var original = UnifiedIssue.FromJira(originalData);
        var modified = UnifiedIssue.FromJira(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        
        // Should have no field differences (only comments changed)
        Assert.Empty(diff.FieldDifferences);
        
        // Should detect one modified comment
        Assert.Single(diff.ModifiedComments);
        Assert.Equal("comment_001", diff.ModifiedComments[0].Id);
        Assert.StartsWith("Modified: ", diff.ModifiedComments[0].Body);
        
        // Should detect one removed comment
        Assert.Single(diff.RemovedComments);
        Assert.Equal("comment_002", diff.RemovedComments[0].Id);
        
        // Should detect one added comment
        Assert.Single(diff.AddedComments);
        Assert.Equal("new_comment_001", diff.AddedComments[0].Id);
        Assert.Equal("modifier@company.com", diff.AddedComments[0].Author);
    }

    [Fact]
    public void FromGitHub_WithComplexNestedData_ExtractsCorrectly()
    {
        // Arrange
        var complexGitHubData = new Dictionary<string, object>
        {
            ["id"] = "complex_gh_001",
            ["number"] = 9999,
            ["title"] = "Complex GitHub Issue",
            ["body"] = "This issue has complex nested data structures.",
            ["state"] = "open",
            ["assignee"] = new Dictionary<string, object>
            {
                ["login"] = "complex-user",
                ["id"] = 555,
                ["url"] = "https://api.github.com/users/complex-user",
                ["type"] = "User"
            },
            ["html_url"] = "https://github.com/test/repo/issues/9999",
            ["created_at"] = DateTime.UtcNow.AddDays(-3),
            ["updated_at"] = DateTime.UtcNow.AddHours(-1),
            ["labels"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = 1001,
                    ["name"] = "bug",
                    ["color"] = "d73a4a",
                    ["description"] = "Something isn't working"
                },
                new Dictionary<string, object>
                {
                    ["id"] = 1002,
                    ["name"] = "priority-high",
                    ["color"] = "ff6b6b"
                }
            },
            ["comments"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "complex_comment_001",
                    ["user"] = new Dictionary<string, object>
                    {
                        ["login"] = "reviewer",
                        ["type"] = "User"
                    },
                    ["body"] = "This is a complex comment with nested user data.",
                    ["created_at"] = DateTime.UtcNow.AddDays(-2),
                    ["updated_at"] = DateTime.UtcNow.AddDays(-2)
                }
            }
        };

        // Act
        var issue = UnifiedIssue.FromGitHub(complexGitHubData);

        // Assert
        Assert.Equal("complex_gh_001", issue.Id);
        Assert.Equal("9999", issue.SourceId);
        Assert.Equal("Complex GitHub Issue", issue.Title);
        Assert.Equal("complex-user", issue.Assignee);
        
        Assert.Equal(2, issue.Labels.Count);
        Assert.Contains("bug", issue.Labels);
        Assert.Contains("priority-high", issue.Labels);
        
        Assert.Single(issue.Comments);
        Assert.Equal("complex_comment_001", issue.Comments[0].Id);
        Assert.Equal("reviewer", issue.Comments[0].Author);
        Assert.Contains("complex comment", issue.Comments[0].Body);
    }

    [Fact]
    public void DiffWith_CustomFieldsRemovedAndAdded_DetectsBoth()
    {
        // Arrange
        var originalData = TestDataFactory.CreateJiraIssue("CUSTOM-001");
        var modifiedData = TestDataFactory.CreateJiraIssue("CUSTOM-001");
        
        var customFields = (Dictionary<string, object>)modifiedData["customFields"];
        
        // Remove existing field
        customFields.Remove("Epic Link");
        
        // Add new field
        customFields["New Requirement"] = "Must be implemented by Q2";
        customFields["Risk Assessment"] = "Low";
        
        var original = UnifiedIssue.FromJira(originalData);
        var modified = UnifiedIssue.FromJira(modifiedData);

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        
        // Check for removed field (Epic Link)
        var removedField = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.Epic Link");
        Assert.NotNull(removedField);
        Assert.Equal("PROJ-100", removedField.OldValue);
        Assert.Null(removedField.NewValue);
        
        // Check for added fields
        var addedField1 = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.New Requirement");
        Assert.NotNull(addedField1);
        Assert.Null(addedField1.OldValue);
        Assert.Equal("Must be implemented by Q2", addedField1.NewValue);
        
        var addedField2 = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CustomFields.Risk Assessment");
        Assert.NotNull(addedField2);
        Assert.Null(addedField2.OldValue);
        Assert.Equal("Low", addedField2.NewValue);
    }

    [Fact]
    public void FromAzureDevOps_WithEmptyTags_CreatesEmptyLabels()
    {
        // Arrange
        var azureData = TestDataFactory.CreateAzureDevOpsWorkItem(1000);
        azureData["tags"] = ""; // Empty tags string

        // Act
        var issue = UnifiedIssue.FromAzureDevOps(azureData);

        // Assert
        Assert.Empty(issue.Labels);
    }

    [Fact]
    public void FromAzureDevOps_WithSingleTag_CreatesSingleLabel()
    {
        // Arrange
        var azureData = TestDataFactory.CreateAzureDevOpsWorkItem(1001);
        azureData["tags"] = "single-tag"; // Single tag without semicolons

        // Act
        var issue = UnifiedIssue.FromAzureDevOps(azureData);

        // Assert
        Assert.Single(issue.Labels);
        Assert.Equal("single-tag", issue.Labels[0]);
    }

    [Fact]
    public void DiffWith_CompletedAtChanges_DetectsCorrectly()
    {
        // Arrange
        var original = UnifiedIssue.FromJira(TestDataFactory.CreateJiraIssue("COMPLETION-001"));
        var modified = UnifiedIssue.FromJira(TestDataFactory.CreateJiraIssue("COMPLETION-001"));
        
        // Set completion time on modified version
        var completionTime = DateTime.UtcNow.AddHours(-2);
        modified.CompletedAt = completionTime;
        modified.UpdatedAt = DateTime.UtcNow;

        // Act
        var diff = original.DiffWith(modified);

        // Assert
        Assert.True(diff.HasChanges);
        
        var completedAtDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "CompletedAt");
        Assert.NotNull(completedAtDiff);
        Assert.Null(completedAtDiff.OldValue);
        Assert.Equal(completionTime, completedAtDiff.NewValue);
    }

    [Fact]
    public void FromJira_WithMalformedDates_UsesCurrentTime()
    {
        // Arrange
        var dataWithBadDates = new Dictionary<string, object>
        {
            ["id"] = "bad_dates_001",
            ["key"] = "BAD-1",
            ["summary"] = "Test with bad dates",
            ["created"] = "not-a-date",
            ["updated"] = "also-not-a-date"
        };

        var beforeTest = DateTime.UtcNow.AddMinutes(-1);

        // Act
        var issue = UnifiedIssue.FromJira(dataWithBadDates);

        // Assert
        var afterTest = DateTime.UtcNow.AddMinutes(1);
        
        Assert.True(issue.CreatedAt >= beforeTest);
        Assert.True(issue.CreatedAt <= afterTest);
        Assert.True(issue.UpdatedAt >= beforeTest);
        Assert.True(issue.UpdatedAt <= afterTest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FromGitHub_WithEmptyOrNullAssignee_SetsEmptyAssignee(string? assigneeLogin)
    {
        // Arrange
        var githubData = TestDataFactory.CreateGitHubIssue(2000);
        
        if (assigneeLogin == null)
        {
            githubData["assignee"] = null!;
        }
        else
        {
            githubData["assignee"] = new Dictionary<string, object>
            {
                ["login"] = assigneeLogin
            };
        }

        // Act
        var issue = UnifiedIssue.FromGitHub(githubData);

        // Assert
        Assert.Equal("", issue.Assignee);
    }

    [Fact]
    public void DiffWith_SameIssueFromDifferentSystems_ShowsSystemAttribution()
    {
        // Arrange
        var jiraData = TestDataFactory.CreateJiraIssue("CROSS-001");
        jiraData["summary"] = "Original Title";
        jiraData["status"] = "Open";

        var githubData = TestDataFactory.CreateGitHubIssue(3000);
        githubData["title"] = "Updated Title";
        githubData["state"] = "closed";

        var jiraIssue = UnifiedIssue.FromJira(jiraData);
        var githubIssue = UnifiedIssue.FromGitHub(githubData);
        
        // Simulate same logical issue with different IDs
        githubIssue.Id = jiraIssue.Id;

        // Act
        var diff = jiraIssue.DiffWith(githubIssue);

        // Assert
        Assert.True(diff.HasChanges);
        
        var titleDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Title");
        Assert.NotNull(titleDiff);
        Assert.Equal(SourceSystem.GitHub, titleDiff.ModifiedBy);
        Assert.Equal("Original Title", titleDiff.OldValue);
        Assert.Equal("Updated Title", titleDiff.NewValue);
        
        var statusDiff = diff.FieldDifferences.FirstOrDefault(d => d.FieldName == "Status");
        Assert.NotNull(statusDiff);
        Assert.Equal(SourceSystem.GitHub, statusDiff.ModifiedBy);
    }
}