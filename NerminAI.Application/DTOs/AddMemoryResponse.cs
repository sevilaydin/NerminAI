namespace NerminAI.Application.DTOs
{
    public class AddMemoryResponse
    {
        public Guid MemoryId { get; set; }
        public Guid? ShadowDocumentId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
