using Microsoft.EntityFrameworkCore;
using Resolve.Domain;

namespace Resolve.Infrastructure;

public class ResolveDbContext : DbContext
{
    public ResolveDbContext(DbContextOptions<ResolveDbContext> options)
        : base(options) { }

    public DbSet<LearningItem> LearningItems => Set<LearningItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LearningItem>(entity =>
        {
            entity.ToTable("learning_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");

            entity.Property(x => x.ItemType).HasColumnName("item_type").HasConversion<string>();

            entity.Property(x => x.Title).HasColumnName("title").IsRequired();

            entity.Property(x => x.Content).HasColumnName("content");

            entity.Property(x => x.ArchivedAt).HasColumnName("archived_at");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        });
    }
}
