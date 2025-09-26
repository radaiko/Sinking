using Sinking.Core.Models;
using Xunit;

namespace Sinking.Core.Tests;

public class SyncJobTests
{
    [Fact]
    public void SyncJob_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var job = new SyncJob
        {
            Name = "Test Job",
            SourceService = SyncServiceType.GitHub,
            TargetService = SyncServiceType.Jira
        };

        // Assert
        Assert.True(job.IsEnabled);
        Assert.False(job.IsBidirectional);
        Assert.True(job.CreatedAt > DateTime.MinValue);
        Assert.True(job.UpdatedAt > DateTime.MinValue);
        Assert.NotNull(job.FieldMappings);
        Assert.NotNull(job.UserMappings);
    }

    [Fact]
    public void SyncServiceType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(1, (int)SyncServiceType.GitHub);
        Assert.Equal(2, (int)SyncServiceType.Jira);
        Assert.Equal(3, (int)SyncServiceType.AzureDevOps);
    }
}
