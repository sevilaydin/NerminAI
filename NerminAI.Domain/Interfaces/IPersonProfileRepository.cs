using NerminAI.Domain.Entities;

namespace NerminAI.Domain.Interfaces
{
    public interface IPersonProfileRepository : IRepository<PersonProfile>
    {
        Task<PersonProfile?> GetActiveProfileAsync();
    }
}
