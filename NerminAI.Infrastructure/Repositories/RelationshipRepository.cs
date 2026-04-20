using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class RelationshipRepository : GenericRepository<Relationship>, IRelationshipRepository
    {
        public RelationshipRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Relationship>> GetByTypeAsync(RelationshipType type)
            => await _dbSet
                .Where(r => r.Type == type)
                .OrderByDescending(r => r.ClosenessLevel)
                .ToListAsync();

        public async Task<IEnumerable<Relationship>> GetByClosenessThresholdAsync(int minLevel)
            => await _dbSet
                .Where(r => r.ClosenessLevel >= minLevel)
                .OrderByDescending(r => r.ClosenessLevel)
                .ToListAsync();
    }
}
