using System.ComponentModel.DataAnnotations;
using Sinking.Core;

namespace Sinking.Web.Data.Models;

/// <summary>
/// Represents a securely stored Personal Access Token for external systems
/// </summary>
public class PersonalAccessToken
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Token name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Token name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Source system is required")]
    public SourceSystem SourceSystem { get; set; }
    
    [Required(ErrorMessage = "Base URL is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Base URL must be between 1 and 200 characters")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted token value - never displayed in plaintext after save
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    [StringLength(2000, ErrorMessage = "Encrypted token exceeds maximum length")]
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
    
    [StringLength(500, ErrorMessage = "Test message exceeds maximum length")]
    public string? LastTestMessage { get; set; }
}