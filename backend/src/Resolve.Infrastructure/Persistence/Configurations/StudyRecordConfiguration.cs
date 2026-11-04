using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resolve.Domain;

namespace Resolve.Infrastructure.Persistence.Configurations;

public sealed class StudyRecordConfiguration : IEntityTypeConfiguration<StudyRecord>
{
    public void Configure(EntityTypeBuilder<StudyRecord> builder)
    {
        builder.ToTable("study_records");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder
            .Property(record => record.LearningItemId)
            .HasColumnName("learning_item_id")
            .IsRequired();
        builder
            .Property(record => record.StudiedAt)
            .HasColumnName("studied_at")
            .IsRequired()
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        builder
            .Property(record => record.Result)
            .HasColumnName("result")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(record => record.DurationSeconds).HasColumnName("duration_seconds");
        builder.Property(record => record.Memo).HasColumnName("memo");
        builder
            .Property(record => record.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        builder
            .Property(record => record.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        builder
            .HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(record => record.LearningItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(record => new { record.LearningItemId, record.StudiedAt });
    }
}
