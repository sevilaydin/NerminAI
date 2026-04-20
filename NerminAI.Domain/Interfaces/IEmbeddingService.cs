namespace NerminAI.Domain.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
        Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts);
        int GetDimension();
    }
}
