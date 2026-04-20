using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class ChunkRepository : GenericRepository<Chunk>, IChunkRepository
    {
        public ChunkRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Chunk>> GetByDocumentIdAsync(Guid documentId)
            => await _dbSet.Where(c => c.DocumentId == documentId).OrderBy(c => c.ChunkIndex).ToListAsync();

        public async Task<int> GetChunkCountAsync()
            => await _dbSet.CountAsync();
    }
}
