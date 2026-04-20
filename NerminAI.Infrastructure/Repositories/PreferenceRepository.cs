using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class PreferenceRepository : GenericRepository<Preference>, IPreferenceRepository
    {
        public PreferenceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Preference>> GetByCategoryAsync(string category)
            => await _dbSet
                .Where(p => p.Category.ToLower() == category.ToLower())
                .OrderBy(p => p.Strength)
                .ToListAsync();
    }
}
