using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class MemoryConfiguration : IEntityTypeConfiguration<Memory>
    {
        public void Configure(EntityTypeBuilder<Memory> builder)
        {
            builder.ToTable("Memories");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Title).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Content).IsRequired();
            builder.Property(m => m.EmotionalTone).HasConversion<int>();
            builder.Property(m => m.Category).HasConversion<int>();

            builder.HasIndex(m => m.Category);
            builder.HasIndex(m => m.MemoryDate);
            builder.HasIndex(m => m.EmotionalTone);
        }
    }
}
