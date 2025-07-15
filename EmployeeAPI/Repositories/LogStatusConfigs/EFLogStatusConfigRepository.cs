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

        public async Task<List<LogStatusConfig>> GetTemplateAsync()
        {
            return await _context.LogStatusConfigs.Where(x => x.IsSystemDefault).ToListAsync();
        }

        public async Task<List<LogStatusConfig>> GetAllAsync(Guid companyId)
        {
            return await _context.LogStatusConfigs.Where(p => p.CompanyId == companyId && !p.IsSystemDefault).ToListAsync();
        }

        public async Task<LogStatusConfig?> GetByIdAsync(Guid id)
        {
            return await _context.LogStatusConfigs.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(LogStatusConfig config)
        {
            _context.LogStatusConfigs.Add(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LogStatusConfig config)
        {
            _context.LogStatusConfigs.Update(config);
            await _context.SaveChangesAsync();
        }
    }
}
