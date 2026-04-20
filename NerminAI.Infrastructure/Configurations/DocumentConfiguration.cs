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
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {

            builder.ToTable("Documents");
            builder.HasKey(d => d.Id);
            builder.Property(d=>d.Title).IsRequired().HasMaxLength(500);
            builder.Property(d=>d.Content).IsRequired();
            builder.Property(d=>d.Type).IsRequired().HasConversion<int>();
            builder.Property(d=>d.CreatedAt).IsRequired();
            builder.Property(d => d.UpdatedAt);

            builder.HasMany(d=>d.Chunks).WithOne(c=>c.Document).HasForeignKey(c=>c.DocumentId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(d => d.Type);
            builder.HasIndex(d => d.CreatedAt);

        }
    }
}
