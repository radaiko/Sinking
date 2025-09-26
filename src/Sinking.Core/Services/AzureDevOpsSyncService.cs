using Microsoft.Extensions.Logging;
using Sinking.Core.Interfaces;
using Sinking.Core.Models;

namespace Sinking.Core.Services;

// Stub implementation of Azure DevOps sync service
// In a full implementation, this would use Microsoft.VisualStudio.Services.Client
public class AzureDevOpsSyncService : ISyncService
{
    private readonly ILogger<AzureDevOpsSyncService> _logger;
    private readonly HttpClient _httpClient;

    public AzureDevOpsSyncService(ILogger<AzureDevOpsSyncService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public SyncServiceType ServiceType => SyncServiceType.AzureDevOps;

    public Task<IEnumerable<SyncableItem>> GetItemsAsync(string configuration)
    {
        // TODO: Implement Azure DevOps work item retrieval
        _logger.LogWarning("Azure DevOps sync service not yet implemented");
        return Task.FromResult(Enumerable.Empty<SyncableItem>());
    }

    public Task<SyncableItem?> GetItemAsync(string configuration, string itemId)
    {
        // TODO: Implement single work item retrieval
        _logger.LogWarning("Azure DevOps sync service not yet implemented");
        return Task.FromResult<SyncableItem?>(null);
    }

    public Task<SyncableItem> CreateItemAsync(string configuration, SyncableItem item)
    {
        // TODO: Implement work item creation
        throw new NotImplementedException("Azure DevOps sync service not yet implemented");
    }

    public Task<SyncableItem> UpdateItemAsync(string configuration, string itemId, SyncableItem item)
    {
        // TODO: Implement work item update
        throw new NotImplementedException("Azure DevOps sync service not yet implemented");
    }

    public Task<IEnumerable<SyncableAttachment>> GetAttachmentsAsync(string configuration, string itemId)
    {
        // TODO: Implement attachment retrieval
        return Task.FromResult(Enumerable.Empty<SyncableAttachment>());
    }

    public Task<SyncableAttachment> AddAttachmentAsync(string configuration, string itemId, SyncableAttachment attachment)
    {
        // TODO: Implement attachment upload
        throw new NotImplementedException("Azure DevOps sync service not yet implemented");
    }

    public Task<string> ResolveUserAsync(string configuration, string userIdentifier)
    {
        // TODO: Implement user resolution
        return Task.FromResult(userIdentifier);
    }
}