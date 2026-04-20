namespace NerminAI.Domain.Entities
{
    public class Embedding : BaseEntity
    {
        public Guid ChunkId { get; set; }
        public float[] Vector { get; set; } = Array.Empty<float>();
        public string Model { get; set; } = "all-MiniLM-L6-v2";

        public Chunk Chunk { get; set; } = null!;
    }
}
