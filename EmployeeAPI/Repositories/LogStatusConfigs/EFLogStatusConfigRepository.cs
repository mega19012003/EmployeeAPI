using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.LogStatusConfigs
{
    public class EFLogStatusConfigRepository : ILogStatusConfigRepository
    {
        private readonly AppDbContext _context;

        public EFLogStatusConfigRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LogStatusConfig>> GetAllAsync()
        {
            return await _context.LogStatusConfigs.ToListAsync();
        }

        public async Task<LogStatusConfig?> GetByIdAsync(int id)
        {
            return await _context.LogStatusConfigs.FindAsync(id);
        }

        public async Task UpdateAsync(LogStatusConfig config)
        {
            _context.LogStatusConfigs.Update(config);
            await _context.SaveChangesAsync();
        }
    }
}
