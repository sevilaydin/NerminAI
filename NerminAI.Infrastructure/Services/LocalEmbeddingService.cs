using NerminAI.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace NerminAI.Infrastructure.Services
{
    public class LocalEmbeddingService : IEmbeddingService
    {
        private const int Dimension = 384;

        public Task<float[]> GenerateEmbeddingAsync(string text)
        {
            return Task.FromResult(GenerateDeterministicEmbedding(text));
        }

        public Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts)
        {
            var results = texts.Select(t => GenerateDeterministicEmbedding(t));
            return Task.FromResult(results);
        }

        public int GetDimension() => Dimension;

        private static float[] GenerateDeterministicEmbedding(string text)
        {
            var embedding = new float[Dimension];
            var bytes = Encoding.UTF8.GetBytes(text.ToLowerInvariant());

            for (int i = 0; i < Dimension; i++)
            {
                var seed = bytes.Select((b, j) => (long)b * (j + 1) * (i + 1)).Sum();
                embedding[i] = (float)Math.Sin(seed * 0.0001) * 0.5f;
            }

            var norm = (float)Math.Sqrt(embedding.Sum(x => x * x));
            if (norm > 0)
                for (int i = 0; i < Dimension; i++)
                    embedding[i] /= norm;

            return embedding;
        }
    }
}
