using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinking.Core.Interfaces;

namespace Sinking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ErrorsController : ControllerBase
{
    private readonly ISyncRepository _repository;

    public ErrorsController(ISyncRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get sync errors
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetErrors(int? jobId = null, int take = 50)
    {
        var errors = await _repository.GetErrorsAsync(jobId, take);
        return Ok(errors);
    }
}