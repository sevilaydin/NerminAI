using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerminAI.Domain.Interfaces
{
    public interface IVectorRepository
    {
        Task<IEnumerable<Chunk>> SearchSimilarChunksAsync(
            float[] queryEmbedding,
            int topK = 5,
            double threshold = 0.7
            );
        Task<Embedding> AddEmbeddingAsync(Guid chunkId, float[] vector, string model);
        Task<bool> HasEmbeddingAsync(Guid chunkId);
    }
}
