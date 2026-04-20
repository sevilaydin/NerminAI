using NerminAI.Domain.Entities;

namespace NerminAI.Domain.Interfaces
{
    public interface IVectorRepository
    {
        Task<IEnumerable<Chunk>> SearchSimilarChunksAsync(
            float[] queryEmbedding,
            int topK = 5,
            double threshold = 0.7);

        Task<IEnumerable<Chunk>> SearchByKeywordsAsync(
            IEnumerable<string> keywords,
            int topK = 3);

        Task<Embedding> AddEmbeddingAsync(Guid chunkId, float[] vector, string model);
        Task<bool> HasEmbeddingAsync(Guid chunkId);
    }
}
