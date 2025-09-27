namespace Sinking.Core.Tests;

/// <summary>
/// Provides realistic test data for different source systems
/// </summary>
public static class TestDataFactory
{
    public static Dictionary<string, object> CreateJiraIssue(string id = "PROJ-123")
    {
        return new Dictionary<string, object>
        {
            ["id"] = "10001",
            ["key"] = id,
            ["summary"] = "Implement user authentication system",
            ["description"] = "We need to implement a robust user authentication system that supports OAuth 2.0, SAML, and traditional username/password authentication. This should include proper session management and security features.",
            ["status"] = "In Progress",
            ["priority"] = "High",
            ["assignee"] = "john.doe@company.com",
            ["created"] = DateTime.UtcNow.AddDays(-10),
            ["updated"] = DateTime.UtcNow.AddDays(-2),
            ["url"] = $"https://company.atlassian.net/browse/{id}",
            ["labels"] = new List<object> { "authentication", "security", "backend" },
            ["comments"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "comment_001",
                    ["author"] = "jane.smith@company.com",
                    ["body"] = "I've researched the OAuth 2.0 implementation. We should use the Authorization Code flow with PKCE for maximum security.",
                    ["created"] = DateTime.UtcNow.AddDays(-8),
                    ["updated"] = DateTime.UtcNow.AddDays(-8)
                },
                new Dictionary<string, object>
                {
                    ["id"] = "comment_002",
                    ["author"] = "john.doe@company.com",
                    ["body"] = "Great research! I'll start with the basic structure and then we can add OAuth support.",
                    ["created"] = DateTime.UtcNow.AddDays(-7),
                    ["updated"] = DateTime.UtcNow.AddDays(-7)
                }
            },
            ["attachments"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "attach_001",
                    ["filename"] = "auth_flow_diagram.png",
                    ["content"] = "https://company.atlassian.net/secure/attachment/10001/auth_flow_diagram.png",
                    ["size"] = 245760L,
                    ["mimeType"] = "image/png",
                    ["created"] = DateTime.UtcNow.AddDays(-9)
                }
            },
            ["customFields"] = new Dictionary<string, object>
            {
                ["Story Points"] = 8,
                ["Epic Link"] = "PROJ-100",
                ["Sprint"] = "Sprint 23",
                ["Business Value"] = "High",
                ["Technical Complexity"] = "Medium"
            }
        };
    }

    public static Dictionary<string, object> CreateModifiedJiraIssue(string id = "PROJ-123")
    {
        var issue = CreateJiraIssue(id);
        issue["summary"] = "Implement user authentication system with 2FA";
        issue["status"] = "In Review";
        issue["priority"] = "Critical";
        issue["updated"] = DateTime.UtcNow;
        issue["assignee"] = "jane.smith@company.com";
        
        // Add a new comment
        var comments = (List<object>)issue["comments"];
        comments.Add(new Dictionary<string, object>
        {
            ["id"] = "comment_003",
            ["author"] = "admin@company.com",
            ["body"] = "Please add two-factor authentication support as well. This is now a critical security requirement.",
            ["created"] = DateTime.UtcNow.AddMinutes(-30),
            ["updated"] = DateTime.UtcNow.AddMinutes(-30)
        });

        // Update custom fields
        var customFields = (Dictionary<string, object>)issue["customFields"];
        customFields["Story Points"] = 13;
        customFields["Technical Complexity"] = "High";
        customFields["Security Review Required"] = true;

        return issue;
    }

    public static Dictionary<string, object> CreateGitHubIssue(int number = 456)
    {
        return new Dictionary<string, object>
        {
            ["id"] = "987654321",
            ["number"] = number,
            ["title"] = "Add REST API documentation",
            ["body"] = "We need comprehensive REST API documentation for our new authentication endpoints. This should include:\n\n- OpenAPI/Swagger specification\n- Code examples in multiple languages\n- Authentication flow diagrams\n- Error response documentation",
            ["state"] = "open",
            ["assignee"] = new Dictionary<string, object>
            {
                ["login"] = "api-docs-team",
                ["id"] = 12345
            },
            ["html_url"] = $"https://github.com/company/project/issues/{number}",
            ["created_at"] = DateTime.UtcNow.AddDays(-5),
            ["updated_at"] = DateTime.UtcNow.AddDays(-1),
            ["labels"] = new List<object>
            {
                new Dictionary<string, object> { ["name"] = "documentation" },
                new Dictionary<string, object> { ["name"] = "api" },
                new Dictionary<string, object> { ["name"] = "high-priority" }
            },
            ["comments"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "gh_comment_001",
                    ["user"] = new Dictionary<string, object> { ["login"] = "developer1" },
                    ["body"] = "I can help with the OpenAPI specification. I've done this for other projects.",
                    ["created_at"] = DateTime.UtcNow.AddDays(-4),
                    ["updated_at"] = DateTime.UtcNow.AddDays(-4)
                }
            },
            ["customFields"] = new Dictionary<string, object>
            {
                ["Milestone"] = "v2.0",
                ["Estimated Hours"] = 16,
                ["Team"] = "Documentation"
            }
        };
    }

    public static Dictionary<string, object> CreateModifiedGitHubIssue(int number = 456)
    {
        var issue = CreateGitHubIssue(number);
        issue["title"] = "Add comprehensive REST API documentation with examples";
        issue["state"] = "closed";
        issue["updated_at"] = DateTime.UtcNow;
        
        // Add new labels
        var labels = (List<object>)issue["labels"];
        labels.Add(new Dictionary<string, object> { ["name"] = "completed" });
        
        // Add new comment
        var comments = (List<object>)issue["comments"];
        comments.Add(new Dictionary<string, object>
        {
            ["id"] = "gh_comment_002",
            ["user"] = new Dictionary<string, object> { ["login"] = "api-docs-team" },
            ["body"] = "Documentation is now complete and published. Closing this issue.",
            ["created_at"] = DateTime.UtcNow.AddMinutes(-15),
            ["updated_at"] = DateTime.UtcNow.AddMinutes(-15)
        });
        
        return issue;
    }

    public static Dictionary<string, object> CreateAzureDevOpsWorkItem(int id = 789)
    {
        return new Dictionary<string, object>
        {
            ["id"] = id.ToString(),
            ["title"] = "Database performance optimization",
            ["description"] = "Optimize database queries for the user management system. Current queries are taking too long and causing timeout issues in production. Focus on:\n\n1. User lookup queries\n2. Permission checking queries\n3. Audit log queries\n4. Add proper indexing",
            ["state"] = "Active",
            ["priority"] = "2",
            ["assignedTo"] = "database.admin@company.com",
            ["createdDate"] = DateTime.UtcNow.AddDays(-12),
            ["changedDate"] = DateTime.UtcNow.AddDays(-3),
            ["url"] = $"https://dev.azure.com/company/project/_workitems/edit/{id}",
            ["tags"] = "performance;database;optimization;production",
            ["comments"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "azure_comment_001",
                    ["createdBy"] = "performance.engineer@company.com",
                    ["text"] = "I've analyzed the slow queries. The main issues are missing indexes on the user_permissions table and inefficient joins in the audit queries.",
                    ["createdDate"] = DateTime.UtcNow.AddDays(-10),
                    ["modifiedDate"] = DateTime.UtcNow.AddDays(-10)
                }
            },
            ["attachments"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "azure_attach_001",
                    ["name"] = "query_performance_report.pdf",
                    ["url"] = "https://dev.azure.com/company/project/_apis/wit/attachments/12345",
                    ["size"] = 1024768L,
                    ["contentType"] = "application/pdf",
                    ["uploadedDate"] = DateTime.UtcNow.AddDays(-11)
                }
            },
            ["customFields"] = new Dictionary<string, object>
            {
                ["Effort"] = 20,
                ["Business Impact"] = "High",
                ["Technical Risk"] = "Medium",
                ["Environment"] = "Production",
                ["Component"] = "Database"
            }
        };
    }

    public static Dictionary<string, object> CreateModifiedAzureDevOpsWorkItem(int id = 789)
    {
        var item = CreateAzureDevOpsWorkItem(id);
        item["title"] = "Database performance optimization - Phase 1 Complete";
        item["state"] = "Resolved";
        item["priority"] = "1";
        item["changedDate"] = DateTime.UtcNow;
        
        // Update tags
        item["tags"] = "performance;database;optimization;production;resolved";
        
        // Add new comment
        var comments = (List<object>)item["comments"];
        comments.Add(new Dictionary<string, object>
        {
            ["id"] = "azure_comment_002",
            ["createdBy"] = "database.admin@company.com",
            ["text"] = "Phase 1 optimization complete. Added indexes and optimized the main queries. Performance improved by 85%. Moving to testing phase.",
            ["createdDate"] = DateTime.UtcNow.AddMinutes(-45),
            ["modifiedDate"] = DateTime.UtcNow.AddMinutes(-45)
        });
        
        // Update custom fields
        var customFields = (Dictionary<string, object>)item["customFields"];
        customFields["Technical Risk"] = "Low";
        customFields["Testing Status"] = "Ready for QA";
        customFields["Performance Improvement"] = "85%";
        
        return item;
    }

    public static Dictionary<string, object> CreateEmptyJiraIssue(string id = "EMPTY-1")
    {
        return new Dictionary<string, object>
        {
            ["id"] = "99999",
            ["key"] = id,
            ["summary"] = "",
            ["description"] = "",
            ["status"] = "",
            ["priority"] = "",
            ["assignee"] = "",
            ["created"] = DateTime.UtcNow,
            ["updated"] = DateTime.UtcNow,
            ["url"] = $"https://company.atlassian.net/browse/{id}",
            ["labels"] = new List<object>(),
            ["comments"] = new List<object>(),
            ["attachments"] = new List<object>(),
            ["customFields"] = new Dictionary<string, object>()
        };
    }

    public static Dictionary<string, object> CreateMalformedJiraIssue()
    {
        return new Dictionary<string, object>
        {
            ["id"] = "invalid_id",
            ["key"] = null!,
            ["summary"] = 12345, // Wrong type
            ["created"] = "invalid-date",
            ["labels"] = "not-a-list", // Wrong type
            ["customFields"] = "not-a-dictionary" // Wrong type
        };
    }
}