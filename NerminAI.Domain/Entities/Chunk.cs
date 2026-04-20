namespace NerminAI.Domain.Entities
{
    public class Chunk : BaseEntity
    {
        public Guid DocumentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int ChunkIndex { get; set; }
        public int TokenCount { get; set; }

        public Document Document { get; set; } = null!;
        public Embedding? Embedding { get; set; }
    }
}
