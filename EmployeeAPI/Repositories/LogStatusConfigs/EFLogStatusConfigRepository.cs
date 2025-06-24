using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.CheckinStatusConfigs
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
            return await _context.CheckinStatusConfigs.ToListAsync();
        }

        public async Task<LogStatusConfig?> GetByIdAsync(int id)
        {
            return await _context.CheckinStatusConfigs.FindAsync(id);
        }

        public async Task UpdateAsync(LogStatusConfig config)
        {
            _context.CheckinStatusConfigs.Update(config);
            await _context.SaveChangesAsync();
        }
    }
}
