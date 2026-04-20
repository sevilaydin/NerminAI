using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Interfaces
{
    public interface IMemoryRepository : IRepository<Memory>
    {
        Task<IEnumerable<Memory>> SearchByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<Memory>> SearchByCategoryAsync(MemoryCategory category);
        Task<IEnumerable<Memory>> GetByEmotionalToneAsync(EmotionalTone tone);
    }
}
