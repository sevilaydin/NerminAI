using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
    {
        public void Configure(EntityTypeBuilder<Relationship> builder)
        {
            builder.ToTable("Relationships");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Description).HasMaxLength(1000);
            builder.Property(r => r.Type).HasConversion<int>();

            builder.HasIndex(r => r.Type);
            builder.HasIndex(r => r.ClosenessLevel);
        }
    }
}
