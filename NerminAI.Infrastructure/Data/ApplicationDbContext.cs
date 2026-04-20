using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;

namespace NerminAI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<Chunk> Chunks { get; set; }
        public DbSet<Embedding> Embeddings { get; set; }

        // Personal AI entities
        public DbSet<PersonProfile> PersonProfiles { get; set; }
        public DbSet<Memory> Memories { get; set; }
        public DbSet<Preference> Preferences { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<LifeEvent> LifeEvents { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<DailyRoutine> DailyRoutines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
