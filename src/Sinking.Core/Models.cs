namespace Sinking.Core;

/// <summary>
/// Represents a comment or note on an issue
/// </summary>
public class IssueComment
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is IssueComment other)
        {
            return Id == other.Id && 
                   Author == other.Author && 
                   Body == other.Body && 
                   CreatedAt == other.CreatedAt && 
                   UpdatedAt == other.UpdatedAt;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Author, Body, CreatedAt, UpdatedAt);
    }
}

/// <summary>
/// Represents an attachment on an issue
/// </summary>
public class IssueAttachment
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is IssueAttachment other)
        {
            return Id == other.Id && 
                   FileName == other.FileName && 
                   Url == other.Url && 
                   Size == other.Size && 
                   ContentType == other.ContentType && 
                   UploadedAt == other.UploadedAt;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FileName, Url, Size, ContentType, UploadedAt);
    }
}