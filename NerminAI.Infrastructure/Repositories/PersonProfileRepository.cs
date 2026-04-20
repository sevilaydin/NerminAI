using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class PersonProfileRepository : GenericRepository<PersonProfile>, IPersonProfileRepository
    {
        public PersonProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PersonProfile?> GetActiveProfileAsync()
            => await _dbSet.FirstOrDefaultAsync(p => p.IsActive);
    }
}
