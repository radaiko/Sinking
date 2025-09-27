namespace Sinking.Core;

/// <summary>
/// Constants for field names used in source system data mapping
/// </summary>
internal static class FieldNames
{
    // Common field names
    public const string Id = "id";
    public const string Title = "title";
    public const string Description = "description";
    public const string Status = "status";
    public const string Priority = "priority";
    public const string Assignee = "assignee";
    public const string CreatedAt = "created_at";
    public const string UpdatedAt = "updated_at";
    public const string Url = "url";
    
    // Jira specific field names
    public static class Jira
    {
        public const string Summary = "summary";
        public const string Key = "key";
        public const string Created = "created";
        public const string Updated = "updated";
        public const string Labels = "labels";
        public const string Comments = "comments";
        public const string Attachments = "attachments";
        public const string CustomFields = "customFields";
        public const string Author = "author";
        public const string Body = "body";
        public const string FileName = "filename";
        public const string Content = "content";
        public const string Size = "size";
        public const string MimeType = "mimeType";
    }
    
    // GitHub specific field names
    public static class GitHub
    {
        public const string Number = "number";
        public const string Body = "body";
        public const string State = "state";
        public const string HtmlUrl = "html_url";
        public const string Labels = "labels";
        public const string Comments = "comments";
        public const string User = "user";
        public const string Login = "login";
        public const string Name = "name";
    }
    
    // Azure DevOps specific field names
    public static class Azure
    {
        public const string State = "state";
        public const string AssignedTo = "assignedTo";
        public const string CreatedDate = "createdDate";
        public const string ChangedDate = "changedDate";
        public const string Tags = "tags";
        public const string Comments = "comments";
        public const string Attachments = "attachments";
        public const string CustomFields = "customFields";
        public const string CreatedBy = "createdBy";
        public const string Text = "text";
        public const string ModifiedDate = "modifiedDate";
        public const string ContentType = "contentType";
        public const string UploadedDate = "uploadedDate";
        public const string FileName = "name";
    }
}