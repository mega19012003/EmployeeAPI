using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.CheckinStatusConfigs
{
    public class EFCheckinStatusConfigRepository : ICheckinStatusConfigRepository
    {
        private readonly AppDbContext _context;

        public EFCheckinStatusConfigRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CheckinStatusConfig>> GetAllAsync()
        {
            return await _context.CheckinStatusConfigs.ToListAsync();
        }

        public async Task<CheckinStatusConfig?> GetByIdAsync(int id)
        {
            return await _context.CheckinStatusConfigs.FindAsync(id);
        }

        public async Task UpdateAsync(CheckinStatusConfig config)
        {
            _context.CheckinStatusConfigs.Update(config);
            await _context.SaveChangesAsync();
        }
    }
}
