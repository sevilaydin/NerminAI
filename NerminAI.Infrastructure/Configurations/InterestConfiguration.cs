using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class InterestConfiguration : IEntityTypeConfiguration<Interest>
    {
        public void Configure(EntityTypeBuilder<Interest> builder)
        {
            builder.ToTable("Interests");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
            builder.Property(i => i.Description).HasMaxLength(1000);
            builder.Property(i => i.Level).HasConversion<int>();
            builder.Property(i => i.RelatedMemoryTitles).HasColumnType("text[]");

            builder.HasIndex(i => i.Level);
        }
    }
}
