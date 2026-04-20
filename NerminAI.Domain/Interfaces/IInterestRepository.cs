using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Interfaces
{
    public interface IInterestRepository : IRepository<Interest>
    {
        Task<IEnumerable<Interest>> GetByLevelAsync(InterestLevel level);
    }
}
