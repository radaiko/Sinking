namespace Sinking.Core;

/// <summary>
/// Handles mapping of comments and attachments from different source systems
/// </summary>
internal static class CommentAndAttachmentMappers
{
    /// <summary>
    /// Maps a Jira comment object to IssueComment
    /// </summary>
    public static IssueComment? MapJiraComment(object comment)
    {
        if (comment is not Dictionary<string, object> commentDict)
            return null;
            
        return new IssueComment
        {
            Id = commentDict.GetValueOrDefault(FieldNames.Id, "")?.ToString() ?? "",
            Author = commentDict.GetValueOrDefault(FieldNames.Jira.Author, "")?.ToString() ?? "",
            Body = commentDict.GetValueOrDefault(FieldNames.Jira.Body, "")?.ToString() ?? "",
            CreatedAt = SourceSystemMappers.ParseDateTime(commentDict.GetValueOrDefault(FieldNames.Jira.Created, DateTime.UtcNow)),
            UpdatedAt = SourceSystemMappers.ParseDateTime(commentDict.GetValueOrDefault(FieldNames.Jira.Updated, DateTime.UtcNow))
        };
    }
    
    /// <summary>
    /// Maps a GitHub comment object to IssueComment
    /// </summary>
    public static IssueComment? MapGitHubComment(object comment)
    {
        if (comment is not Dictionary<string, object> commentDict)
            return null;
            
        return new IssueComment
        {
            Id = commentDict.GetValueOrDefault(FieldNames.Id, "")?.ToString() ?? "",
            Author = SourceSystemMappers.ExtractGitHubCommentAuthor(commentDict.GetValueOrDefault(FieldNames.GitHub.User, null!)),
            Body = commentDict.GetValueOrDefault(FieldNames.GitHub.Body, "")?.ToString() ?? "",
            CreatedAt = SourceSystemMappers.ParseDateTime(commentDict.GetValueOrDefault(FieldNames.CreatedAt, DateTime.UtcNow)),
            UpdatedAt = SourceSystemMappers.ParseDateTime(commentDict.GetValueOrDefault(FieldNames.UpdatedAt, DateTime.UtcNow))
        };
    }
    
    /// <summary>
    /// Maps an Azure DevOps comment object to IssueComment
    /// </summary>
    public static IssueComment? MapAzureComment(object comment)
    {
        if (comment is not Dictionary<string, object> commentDict)
            return null;
            
        return new IssueComment
        {
            Id = commentDict.GetValueOrDefault(FieldNames.Id, "")?.ToString() ?? "",
            Author = commentDict.GetValueOrDefault(FieldNames.Azure.CreatedBy, "")?.ToString() ?? "",
            Body = commentDict.GetValueOrDefault(FieldNames.Azure.Text, "")?.ToString() ?? "",
            CreatedAt = SourceSystemMappers.ParseDateTime(commentDict.GetValueOrDefault(FieldNames.Azure.CreatedDate, DateTime.UtcNow)),
            UpdatedAt = SourceSystemMappers.ParseDateTime(commentDict.GetValueOrDefault(FieldNames.Azure.ModifiedDate, DateTime.UtcNow))
        };
    }
    
    /// <summary>
    /// Maps a Jira attachment object to IssueAttachment
    /// </summary>
    public static IssueAttachment? MapJiraAttachment(object attachment)
    {
        if (attachment is not Dictionary<string, object> attachmentDict)
            return null;
            
        return new IssueAttachment
        {
            Id = attachmentDict.GetValueOrDefault(FieldNames.Id, "")?.ToString() ?? "",
            FileName = attachmentDict.GetValueOrDefault(FieldNames.Jira.FileName, "")?.ToString() ?? "",
            Url = attachmentDict.GetValueOrDefault(FieldNames.Jira.Content, "")?.ToString() ?? "",
            Size = Convert.ToInt64(attachmentDict.GetValueOrDefault(FieldNames.Jira.Size, 0L)),
            ContentType = attachmentDict.GetValueOrDefault(FieldNames.Jira.MimeType, "")?.ToString() ?? "",
            UploadedAt = SourceSystemMappers.ParseDateTime(attachmentDict.GetValueOrDefault(FieldNames.Jira.Created, DateTime.UtcNow))
        };
    }
    
    /// <summary>
    /// Maps an Azure DevOps attachment object to IssueAttachment
    /// </summary>
    public static IssueAttachment? MapAzureAttachment(object attachment)
    {
        if (attachment is not Dictionary<string, object> attachmentDict)
            return null;
            
        return new IssueAttachment
        {
            Id = attachmentDict.GetValueOrDefault(FieldNames.Id, "")?.ToString() ?? "",
            FileName = attachmentDict.GetValueOrDefault(FieldNames.Azure.FileName, "")?.ToString() ?? "",
            Url = attachmentDict.GetValueOrDefault(FieldNames.Url, "")?.ToString() ?? "",
            Size = Convert.ToInt64(attachmentDict.GetValueOrDefault(FieldNames.Jira.Size, 0L)),
            ContentType = attachmentDict.GetValueOrDefault(FieldNames.Azure.ContentType, "")?.ToString() ?? "",
            UploadedAt = SourceSystemMappers.ParseDateTime(attachmentDict.GetValueOrDefault(FieldNames.Azure.UploadedDate, DateTime.UtcNow))
        };
    }
}