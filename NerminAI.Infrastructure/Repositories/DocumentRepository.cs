using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type)
            => await _dbSet.Where(d => d.Type == type).ToListAsync();

        public async Task<Document?> GetWithChunksAsync(Guid id)
            => await _dbSet
                .Include(d => d.Chunks)
                    .ThenInclude(c => c.Embedding)
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<int> GetDocumentCountAsync()
            => await _dbSet.CountAsync();
    }
}
