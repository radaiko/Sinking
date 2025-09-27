using Sinking.Web.Data.Models;

namespace Sinking.Web.Services;

/// <summary>
/// Service for logging changes for CRA compliance
/// </summary>
public interface IChangeLogService
{
    /// <summary>
    /// Log a change made through the web UI
    /// </summary>
    /// <param name="entityType">Type of entity changed</param>
    /// <param name="entityId">ID of the entity changed</param>
    /// <param name="changeType">Type of change made</param>
    /// <param name="description">Description of the change</param>
    /// <param name="userId">User who made the change</param>
    /// <param name="oldValues">Previous values (optional)</param>
    /// <param name="newValues">New values (optional)</param>
    /// <param name="ipAddress">IP address of the user</param>
    /// <param name="userAgent">User agent of the browser</param>
    Task LogChangeAsync(
        string entityType,
        int entityId,
        string changeType,
        string description,
        string userId,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null);
    
    /// <summary>
    /// Get change history for a specific entity
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>List of changes for the entity</returns>
    Task<List<ChangeLog>> GetEntityChangeHistoryAsync(string entityType, int entityId);
    
    /// <summary>
    /// Get all changes made by a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size for pagination</param>
    /// <returns>List of changes made by the user</returns>
    Task<List<ChangeLog>> GetUserChangesAsync(string userId, int pageNumber = 1, int pageSize = 50);
}