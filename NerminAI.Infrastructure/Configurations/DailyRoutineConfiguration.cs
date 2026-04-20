using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class DailyRoutineConfiguration : IEntityTypeConfiguration<DailyRoutine>
    {
        public void Configure(EntityTypeBuilder<DailyRoutine> builder)
        {
            builder.ToTable("DailyRoutines");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.TimeOfDay).IsRequired().HasMaxLength(50);
            builder.Property(d => d.Activity).IsRequired().HasMaxLength(300);
            builder.Property(d => d.Frequency).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Notes).HasMaxLength(1000);

            builder.HasIndex(d => d.TimeOfDay);
        }
    }
}
