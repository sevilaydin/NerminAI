using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class MemoryRepository : GenericRepository<Memory>, IMemoryRepository
    {
        public MemoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Memory>> SearchByDateRangeAsync(DateTime from, DateTime to)
            => await _dbSet
                .Where(m => m.MemoryDate >= from && m.MemoryDate <= to)
                .OrderBy(m => m.MemoryDate)
                .ToListAsync();

        public async Task<IEnumerable<Memory>> SearchByCategoryAsync(MemoryCategory category)
            => await _dbSet
                .Where(m => m.Category == category)
                .OrderByDescending(m => m.MemoryDate)
                .ToListAsync();

        public async Task<IEnumerable<Memory>> GetByEmotionalToneAsync(EmotionalTone tone)
            => await _dbSet
                .Where(m => m.EmotionalTone == tone)
                .OrderByDescending(m => m.MemoryDate)
                .ToListAsync();
    }
}
