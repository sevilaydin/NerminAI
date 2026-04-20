using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Interfaces
{
    public interface IRelationshipRepository : IRepository<Relationship>
    {
        Task<IEnumerable<Relationship>> GetByTypeAsync(RelationshipType type);
        Task<IEnumerable<Relationship>> GetByClosenessThresholdAsync(int minLevel);
    }
}
