using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class LifeEventRepository : GenericRepository<LifeEvent>, ILifeEventRepository
    {
        public LifeEventRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LifeEvent>> GetByDateRangeAsync(DateTime from, DateTime to)
            => await _dbSet
                .Where(e => e.Date >= from && e.Date <= to)
                .OrderBy(e => e.Date)
                .ToListAsync();

        public async Task<IEnumerable<LifeEvent>> GetByTagsAsync(IEnumerable<string> tags)
        {
            var tagList = tags.Select(t => t.ToLower()).ToList();
            var all = await _dbSet.ToListAsync();
            return all
                .Where(e => e.Tags.Any(t => tagList.Contains(t.ToLower())))
                .OrderByDescending(e => e.EmotionalSignificance);
        }

        public async Task<IEnumerable<LifeEvent>> GetBySignificanceThresholdAsync(int minLevel)
            => await _dbSet
                .Where(e => e.EmotionalSignificance >= minLevel)
                .OrderByDescending(e => e.EmotionalSignificance)
                .ToListAsync();
    }
}
