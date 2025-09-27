using System.ComponentModel.DataAnnotations;
using Sinking.Core;

namespace Sinking.Web.Data.Models;

/// <summary>
/// Represents a securely stored Personal Access Token for external systems
/// </summary>
public class PersonalAccessToken
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public SourceSystem SourceSystem { get; set; }
    
    [Required]
    [StringLength(200)]
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted token value - never displayed in plaintext after save
    /// </summary>
    [Required]
    public string EncryptedToken { get; set; } = string.Empty;
    
    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Test connection result and timestamp
    /// </summary>
    public bool? LastTestResult { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestMessage { get; set; }
}