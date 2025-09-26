using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinking.Core.Interfaces;
using Sinking.Core.Models;

namespace Sinking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncJobsController : ControllerBase
{
    private readonly ISyncRepository _repository;
    private readonly ISyncOrchestrator _orchestrator;
    private readonly ILogger<SyncJobsController> _logger;

    public SyncJobsController(
        ISyncRepository repository, 
        ISyncOrchestrator orchestrator,
        ILogger<SyncJobsController> logger)
    {
        _repository = repository;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Get all sync jobs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SyncJob>>> GetJobs()
    {
        var jobs = await _repository.GetActiveJobsAsync();
        return Ok(jobs);
    }

    /// <summary>
    /// Get a specific sync job
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SyncJob>> GetJob(int id)
    {
        var job = await _repository.GetJobAsync(id);
        if (job == null)
        {
            return NotFound();
        }
        return Ok(job);
    }

    /// <summary>
    /// Create a new sync job
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SyncJob>> CreateJob(CreateSyncJobRequest request)
    {
        var job = new SyncJob
        {
            Name = request.Name,
            Description = request.Description,
            SourceService = request.SourceService,
            TargetService = request.TargetService,
            SourceConfiguration = request.SourceConfiguration,
            TargetConfiguration = request.TargetConfiguration,
            IsEnabled = request.IsEnabled,
            IsBidirectional = request.IsBidirectional,
            CronSchedule = request.CronSchedule
        };

        // Add field mappings
        if (request.FieldMappings != null)
        {
            job.FieldMappings.AddRange(request.FieldMappings.Select(fm => new FieldMapping
            {
                SourceField = fm.SourceField,
                TargetField = fm.TargetField,
                TransformExpression = fm.TransformExpression,
                IsRequired = fm.IsRequired
            }));
        }

        // Add user mappings
        if (request.UserMappings != null)
        {
            job.UserMappings.AddRange(request.UserMappings.Select(um => new UserMapping
            {
                SourceUser = um.SourceUser,
                TargetUser = um.TargetUser
            }));
        }

        var createdJob = await _repository.CreateJobAsync(job);
        return CreatedAtAction(nameof(GetJob), new { id = createdJob.Id }, createdJob);
    }

    /// <summary>
    /// Update an existing sync job
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SyncJob>> UpdateJob(int id, UpdateSyncJobRequest request)
    {
        var job = await _repository.GetJobAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        job.Name = request.Name;
        job.Description = request.Description;
        job.IsEnabled = request.IsEnabled;
        job.IsBidirectional = request.IsBidirectional;
        job.CronSchedule = request.CronSchedule;

        var updatedJob = await _repository.UpdateJobAsync(job);
        return Ok(updatedJob);
    }

    /// <summary>
    /// Delete a sync job
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var deleted = await _repository.DeleteJobAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Manually trigger a sync job
    /// </summary>
    [HttpPost("{id}/sync")]
    public async Task<IActionResult> TriggerSync(int id)
    {
        var job = await _repository.GetJobAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        // Start sync in background
        _ = Task.Run(() => _orchestrator.SyncJobAsync(id));

        return Accepted(new { message = "Sync job started", jobId = id });
    }

    /// <summary>
    /// Get sync job runs history
    /// </summary>
    [HttpGet("{id}/runs")]
    public async Task<ActionResult<IEnumerable<SyncJobRun>>> GetJobRuns(int id, int take = 10)
    {
        var runs = await _repository.GetJobRunsAsync(id, take);
        return Ok(runs);
    }

    /// <summary>
    /// Get sync items for a job
    /// </summary>
    [HttpGet("{id}/items")]
    public async Task<ActionResult<IEnumerable<SyncItem>>> GetSyncItems(int id)
    {
        var items = await _repository.GetSyncItemsAsync(id);
        return Ok(items);
    }
}

// DTOs for API requests
public record CreateSyncJobRequest(
    string Name,
    string? Description,
    SyncServiceType SourceService,
    SyncServiceType TargetService,
    string SourceConfiguration,
    string TargetConfiguration,
    bool IsEnabled = true,
    bool IsBidirectional = false,
    string? CronSchedule = null,
    List<CreateFieldMappingRequest>? FieldMappings = null,
    List<CreateUserMappingRequest>? UserMappings = null
);

public record UpdateSyncJobRequest(
    string Name,
    string? Description,
    bool IsEnabled,
    bool IsBidirectional,
    string? CronSchedule
);

public record CreateFieldMappingRequest(
    string SourceField,
    string TargetField,
    string? TransformExpression = null,
    bool IsRequired = false
);

public record CreateUserMappingRequest(
    string SourceUser,
    string TargetUser
);