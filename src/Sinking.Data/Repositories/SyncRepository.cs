using Microsoft.EntityFrameworkCore;
using Sinking.Core.Interfaces;
using Sinking.Core.Models;

namespace Sinking.Data.Repositories;

public class SyncRepository : ISyncRepository
{
    private readonly SinkingDbContext _context;

    public SyncRepository(SinkingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SyncJob>> GetActiveJobsAsync()
    {
        return await _context.SyncJobs
            .Where(j => j.IsEnabled)
            .Include(j => j.FieldMappings)
            .Include(j => j.UserMappings)
            .ToListAsync();
    }

    public async Task<SyncJob?> GetJobAsync(int id)
    {
        return await _context.SyncJobs
            .Include(j => j.FieldMappings)
            .Include(j => j.UserMappings)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<SyncJob> CreateJobAsync(SyncJob job)
    {
        _context.SyncJobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<SyncJob> UpdateJobAsync(SyncJob job)
    {
        job.UpdatedAt = DateTime.UtcNow;
        _context.SyncJobs.Update(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        var job = await _context.SyncJobs.FindAsync(id);
        if (job == null) return false;

        _context.SyncJobs.Remove(job);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SyncJobRun> CreateJobRunAsync(SyncJobRun run)
    {
        _context.SyncJobRuns.Add(run);
        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<SyncJobRun> UpdateJobRunAsync(SyncJobRun run)
    {
        _context.SyncJobRuns.Update(run);
        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<IEnumerable<SyncJobRun>> GetJobRunsAsync(int jobId, int take = 10)
    {
        return await _context.SyncJobRuns
            .Where(r => r.SyncJobId == jobId)
            .OrderByDescending(r => r.StartTime)
            .Take(take)
            .ToListAsync();
    }

    public async Task<SyncItem?> GetSyncItemAsync(int jobId, string sourceItemId)
    {
        return await _context.SyncItems
            .FirstOrDefaultAsync(i => i.SyncJobId == jobId && i.SourceItemId == sourceItemId);
    }

    public async Task<SyncItem> UpsertSyncItemAsync(SyncItem item)
    {
        var existing = await _context.SyncItems
            .FirstOrDefaultAsync(i => i.SyncJobId == item.SyncJobId && i.SourceItemId == item.SourceItemId);

        if (existing != null)
        {
            existing.TargetItemId = item.TargetItemId;
            existing.SourceData = item.SourceData;
            existing.TargetData = item.TargetData;
            existing.LastSyncTime = item.LastSyncTime;
            existing.SourceLastModified = item.SourceLastModified;
            existing.TargetLastModified = item.TargetLastModified;
            existing.Status = item.Status;
            
            _context.SyncItems.Update(existing);
        }
        else
        {
            _context.SyncItems.Add(item);
        }

        await _context.SaveChangesAsync();
        return existing ?? item;
    }

    public async Task<IEnumerable<SyncItem>> GetSyncItemsAsync(int jobId)
    {
        return await _context.SyncItems
            .Where(i => i.SyncJobId == jobId)
            .OrderByDescending(i => i.LastSyncTime)
            .ToListAsync();
    }

    public async Task<SyncError> CreateErrorAsync(SyncError error)
    {
        _context.SyncErrors.Add(error);
        await _context.SaveChangesAsync();
        return error;
    }

    public async Task<IEnumerable<SyncError>> GetErrorsAsync(int? jobId = null, int take = 50)
    {
        var query = _context.SyncErrors.AsQueryable();

        if (jobId.HasValue)
        {
            query = query.Where(e => e.SyncJobId == jobId.Value);
        }

        return await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(take)
            .ToListAsync();
    }
}