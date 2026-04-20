using NerminAI.Domain.Entities;

namespace NerminAI.Domain.Interfaces
{
    public interface IPreferenceRepository : IRepository<Preference>
    {
        Task<IEnumerable<Preference>> GetByCategoryAsync(string category);
    }
}
