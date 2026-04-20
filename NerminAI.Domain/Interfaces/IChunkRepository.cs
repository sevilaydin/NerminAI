using NerminAI.Domain.Entities;

namespace NerminAI.Domain.Interfaces
{
    public interface IChunkRepository : IRepository<Chunk>
    {
        Task<IEnumerable<Chunk>> GetByDocumentIdAsync(Guid documentId);
        Task<int> GetChunkCountAsync();
    }
}
