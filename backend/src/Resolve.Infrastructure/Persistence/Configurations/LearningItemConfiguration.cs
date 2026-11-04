using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resolve.Domain;

namespace Resolve.Infrastructure.Persistence.Configurations;

public sealed class LearningItemConfiguration : IEntityTypeConfiguration<LearningItem>
{
    public void Configure(EntityTypeBuilder<LearningItem> builder)
    {
        builder.ToTable("learning_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ItemType).HasColumnName("item_type").HasConversion<string>();
        builder.Property(x => x.Title).HasColumnName("title").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content");
        builder
            .Property(x => x.ArchivedAt)
            .HasColumnName("archived_at")
            .HasConversion(
                value => value,
                value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null
            );
        builder
            .Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        builder
            .Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
