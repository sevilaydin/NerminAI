using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class InterestRepository : GenericRepository<Interest>, IInterestRepository
    {
        public InterestRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Interest>> GetByLevelAsync(InterestLevel level)
            => await _dbSet
                .Where(i => i.Level == level)
                .OrderBy(i => i.Name)
                .ToListAsync();
    }
}
