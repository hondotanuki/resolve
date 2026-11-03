using Microsoft.EntityFrameworkCore;
using Resolve.Domain;
using Resolve.Infrastructure.Persistence.Configurations;

namespace Resolve.Infrastructure.Persistence;

public class ResolveDbContext : DbContext
{
    public ResolveDbContext(DbContextOptions<ResolveDbContext> options)
        : base(options) { }

    public DbSet<LearningItem> LearningItems => Set<LearningItem>();
    public DbSet<StudyRecord> StudyRecords => Set<StudyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new LearningItemConfiguration());
        modelBuilder.ApplyConfiguration(new StudyRecordConfiguration());
    }
}
