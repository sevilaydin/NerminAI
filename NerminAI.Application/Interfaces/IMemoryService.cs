using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface IMemoryService
    {
        Task<AddMemoryResponse> AddMemoryAsync(AddMemoryRequest request);
        Task<MemoryReflectionResponse> ReflectAsync(MemoryReflectionRequest request);
    }
}
