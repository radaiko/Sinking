using Microsoft.EntityFrameworkCore;
using Sinking.Core.Models;

namespace Sinking.Data;

public class SinkingDbContext : DbContext
{
    public SinkingDbContext(DbContextOptions<SinkingDbContext> options) : base(options)
    {
    }

    public DbSet<SyncJob> SyncJobs { get; set; }
    public DbSet<SyncJobRun> SyncJobRuns { get; set; }
    public DbSet<SyncItem> SyncItems { get; set; }
    public DbSet<SyncError> SyncErrors { get; set; }
    public DbSet<FieldMapping> FieldMappings { get; set; }
    public DbSet<UserMapping> UserMappings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SyncJob
        modelBuilder.Entity<SyncJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CronSchedule).HasMaxLength(100);
            entity.Property(e => e.SourceConfiguration).HasColumnType("TEXT");
            entity.Property(e => e.TargetConfiguration).HasColumnType("TEXT");
            entity.HasMany(e => e.FieldMappings).WithOne(e => e.SyncJob).HasForeignKey(e => e.SyncJobId);
            entity.HasMany(e => e.UserMappings).WithOne(e => e.SyncJob).HasForeignKey(e => e.SyncJobId);
        });

        // Configure SyncJobRun
        modelBuilder.Entity<SyncJobRun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.SyncJob).WithMany().HasForeignKey(e => e.SyncJobId);
            entity.Property(e => e.LogMessage).HasColumnType("TEXT");
        });

        // Configure SyncItem
        modelBuilder.Entity<SyncItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.SyncJob).WithMany().HasForeignKey(e => e.SyncJobId);
            entity.Property(e => e.SourceItemId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetItemId).HasMaxLength(100);
            entity.Property(e => e.SourceData).HasColumnType("TEXT");
            entity.Property(e => e.TargetData).HasColumnType("TEXT");
            entity.HasIndex(e => new { e.SyncJobId, e.SourceItemId }).IsUnique();
        });

        // Configure SyncError
        modelBuilder.Entity<SyncError>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.SyncJob).WithMany().HasForeignKey(e => e.SyncJobId);
            entity.Property(e => e.ErrorMessage).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.StackTrace).HasColumnType("TEXT");
            entity.Property(e => e.Context).HasColumnType("TEXT");
        });

        // Configure FieldMapping
        modelBuilder.Entity<FieldMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceField).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetField).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TransformExpression).HasMaxLength(500);
        });

        // Configure UserMapping
        modelBuilder.Entity<UserMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceUser).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TargetUser).IsRequired().HasMaxLength(200);
        });
    }
}
