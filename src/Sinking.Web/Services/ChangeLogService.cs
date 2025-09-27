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

    public ChangeLogService(ApplicationDbContext context)
    {
        _context = context;
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
        string? userAgent = null)
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
        await _context.SaveChangesAsync();
    }

    public async Task<List<ChangeLog>> GetEntityChangeHistoryAsync(string entityType, int entityId)
    {
        return await _context.ChangeLogs
            .Include(c => c.User)
            .Where(c => c.EntityType == entityType && c.EntityId == entityId)
            .OrderByDescending(c => c.ChangedAt)
            .ToListAsync();
    }

    public async Task<List<ChangeLog>> GetUserChangesAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        return await _context.ChangeLogs
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ChangedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}