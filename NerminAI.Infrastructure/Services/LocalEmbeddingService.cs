using System.Text;
using System.Text.Json;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Infrastructure.Services
{
    public class LocalEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelName;
        private const int Dimension = 384;

        public LocalEmbeddingService(HttpClient httpClient, string modelName = "all-MiniLM-L6-v2")
        {
            _httpClient = httpClient;
            _modelName = modelName;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var embeddings = await GenerateEmbeddingsAsync(new[] { text });
            return embeddings.First();
        }

        public async Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts)
        {
            var request = new { texts = texts.ToArray() };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/embed", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);

            return result?.Embeddings ?? Array.Empty<float[]>();
        }

        public int GetDimension() => Dimension;

        private class EmbeddingResponse
        {
            public float[][]? Embeddings { get; set; }
        }
    }
}
