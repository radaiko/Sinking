using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sinking.Web.Data.Models;

namespace Sinking.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<PersonalAccessToken> PersonalAccessTokens => Set<PersonalAccessToken>();
    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();
    public DbSet<SyncJobExecution> SyncJobExecutions => Set<SyncJobExecution>();
    public DbSet<SyncJobFailure> SyncJobFailures => Set<SyncJobFailure>();
    public DbSet<FieldMapping> FieldMappings => Set<FieldMapping>();
    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure PersonalAccessToken relationships
        builder.Entity<PersonalAccessToken>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SyncJob relationships
        builder.Entity<SyncJob>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SyncJob>()
            .HasOne(s => s.SourceToken)
            .WithMany()
            .HasForeignKey(s => s.SourceTokenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SyncJob>()
            .HasOne(s => s.TargetToken)
            .WithMany()
            .HasForeignKey(s => s.TargetTokenId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure SyncJobExecution relationships
        builder.Entity<SyncJobExecution>()
            .HasOne(e => e.SyncJob)
            .WithMany(s => s.Executions)
            .HasForeignKey(e => e.SyncJobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SyncJobFailure relationships
        builder.Entity<SyncJobFailure>()
            .HasOne(f => f.Execution)
            .WithMany(e => e.Failures)
            .HasForeignKey(f => f.ExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure FieldMapping relationships
        builder.Entity<FieldMapping>()
            .HasOne(f => f.SyncJob)
            .WithMany(s => s.FieldMappings)
            .HasForeignKey(f => f.SyncJobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ChangeLog relationships
        builder.Entity<ChangeLog>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add indexes for performance
        builder.Entity<PersonalAccessToken>()
            .HasIndex(p => p.UserId);

        builder.Entity<SyncJob>()
            .HasIndex(s => s.UserId);

        builder.Entity<SyncJob>()
            .HasIndex(s => s.IsActive);

        builder.Entity<SyncJobExecution>()
            .HasIndex(e => e.Status);

        builder.Entity<ChangeLog>()
            .HasIndex(c => c.ChangedAt);

        builder.Entity<ChangeLog>()
            .HasIndex(c => c.UserId);
    }
}
