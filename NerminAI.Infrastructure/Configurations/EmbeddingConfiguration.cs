using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Configurations
{
    public class EmbeddingConfiguration : IEntityTypeConfiguration<Embedding>
    {
        public void Configure(EntityTypeBuilder<Embedding> builder)
        {
            builder.ToTable("Embeddings");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.ChunkId).IsRequired();
            builder.Property(e => e.Vector).IsRequired().HasColumnType("real[]");
            builder.Property(e => e.Model).IsRequired().HasMaxLength(100);
            builder.Property(e => e.CreatedAt).IsRequired();

            builder.HasOne(e => e.Chunk)
                .WithOne(c => c.Embedding)
                .HasForeignKey<Embedding>(e => e.ChunkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.ChunkId).IsUnique();
        }
    }
}
