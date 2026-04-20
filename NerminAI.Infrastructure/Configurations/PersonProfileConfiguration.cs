using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class PersonProfileConfiguration : IEntityTypeConfiguration<PersonProfile>
    {
        public void Configure(EntityTypeBuilder<PersonProfile> builder)
        {
            builder.ToTable("PersonProfiles");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.FullName).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Bio).IsRequired();
            builder.Property(p => p.Location).HasMaxLength(300);
            builder.Property(p => p.ToneStyle).HasMaxLength(300);
            builder.Property(p => p.SpeakingStyle).HasMaxLength(100);

            // Store string[] as PostgreSQL text[] array (Npgsql handles this natively)
            builder.Property(p => p.PersonalityTraits).HasColumnType("text[]");

            // Enforce at most one active profile via partial unique index
            builder.HasIndex(p => p.IsActive)
                   .HasFilter("\"IsActive\" = true")
                   .IsUnique();
        }
    }
}
