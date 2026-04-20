using NerminAI.Domain.Entities;

namespace NerminAI.Domain.Interfaces
{
    public interface ILifeEventRepository : IRepository<LifeEvent>
    {
        Task<IEnumerable<LifeEvent>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<LifeEvent>> GetByTagsAsync(IEnumerable<string> tags);
        Task<IEnumerable<LifeEvent>> GetBySignificanceThresholdAsync(int minLevel);
    }
}
