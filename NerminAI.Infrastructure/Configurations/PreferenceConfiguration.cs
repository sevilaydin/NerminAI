using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class PreferenceConfiguration : IEntityTypeConfiguration<Preference>
    {
        public void Configure(EntityTypeBuilder<Preference> builder)
        {
            builder.ToTable("Preferences");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Category).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Item).IsRequired().HasMaxLength(300);
            builder.Property(p => p.Context).HasMaxLength(500);
            builder.Property(p => p.Strength).HasConversion<int>();

            builder.HasIndex(p => p.Category);
            builder.HasIndex(p => p.Strength);
        }
    }
}
