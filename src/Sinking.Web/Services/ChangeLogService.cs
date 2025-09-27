using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sinking.Web.Data;
using Sinking.Web.Data.Models;

namespace Sinking.Web.Services;

/// <summary>
/// Service for logging changes for CRA compliance
/// </summary>
public class ChangeLogService : IChangeLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChangeLogService> _logger;

    public ChangeLogService(ApplicationDbContext context, ILogger<ChangeLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogChangeAsync(
        string entityType,
        int entityId,
        string changeType,
        string description,
        string userId,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeType);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        
        if (entityId <= 0)
            throw new ArgumentOutOfRangeException(nameof(entityId), "Entity ID must be positive");

        try
        {
            var changeLog = new ChangeLog
            {
                EntityType = entityType,
                EntityId = entityId,
                ChangeType = changeType,
                Description = description,
                UserId = userId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                ChangedAt = DateTime.UtcNow
            };

            _context.ChangeLogs.Add(changeLog);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            
            _logger.LogInformation("Change logged: {EntityType} {EntityId} - {ChangeType} by user {UserId}", 
                entityType, entityId, changeType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log change for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    public async Task<List<ChangeLog>> GetEntityChangeHistoryAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        
        if (entityId <= 0)
            throw new ArgumentOutOfRangeException(nameof(entityId), "Entity ID must be positive");
            
        try
        {
            return await _context.ChangeLogs
                .Include(c => c.User)
                .Where(c => c.EntityType == entityType && c.EntityId == entityId)
                .OrderByDescending(c => c.ChangedAt)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve change history for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    public async Task<List<ChangeLog>> GetUserChangesAsync(string userId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 1000) pageSize = 50;

            return await _context.ChangeLogs
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.ChangedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user changes for {UserId}", userId);
            throw;
        }
    }
}