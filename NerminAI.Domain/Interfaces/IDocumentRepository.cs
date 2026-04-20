using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Interfaces
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type);
        Task<Document?> GetWithChunksAsync(Guid id);
        Task<int> GetDocumentCountAsync();
    }
}
