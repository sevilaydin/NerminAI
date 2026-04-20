using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class LifeEventConfiguration : IEntityTypeConfiguration<LifeEvent>
    {
        public void Configure(EntityTypeBuilder<LifeEvent> builder)
        {
            builder.ToTable("LifeEvents");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
            builder.Property(e => e.Description).IsRequired();
            builder.Property(e => e.Location).HasMaxLength(300);
            builder.Property(e => e.Tags).HasColumnType("text[]");

            builder.HasIndex(e => e.Date);
            builder.HasIndex(e => e.EmotionalSignificance);
        }
    }
}
