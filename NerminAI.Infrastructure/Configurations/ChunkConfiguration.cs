using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerminAI.Infrastructure.Configurations
{
    public class ChunkConfiguration : IEntityTypeConfiguration<Chunk>
    {
        public void Configure(EntityTypeBuilder<Chunk> builder)
        {
            builder.ToTable("Chunks");
            builder.HasKey(c=> c.Id);
            builder.Property(c => c.Content).IsRequired();
            builder.Property(c=>c.ChunkIndex).IsRequired();
            builder.Property(c=>c.TokenCount).IsRequired();
            builder.Property(c=>c.DocumentId).IsRequired();
            builder.Property(c=>c.CreatedAt).IsRequired();

            builder.HasOne(c => c.Document).WithMany(d=>d.Chunks).HasForeignKey(c=>c.DocumentId);

            builder.HasIndex(c => c.DocumentId);
            builder.HasIndex(c => new { c.DocumentId, c.ChunkIndex });

        }
    }
}
