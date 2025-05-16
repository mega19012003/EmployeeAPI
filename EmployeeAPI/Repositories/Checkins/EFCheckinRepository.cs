using System.Linq.Expressions;
using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Checkins
{
    public class EFCheckinRepository : ICheckinRepository
    {
        private readonly AppDbContext _context;

        public EFCheckinRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Checkin>> GetAllAsync()
        {
            return await _context.Checkins
                .Include(c => c.Staff)
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<Checkin> GetByIdAsync(Guid id)
        {
            return await _context.Checkins
                .Include(c => c.Staff)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task CreateAsync(Checkin checkin)
        {
            _context.Checkins.Add(checkin);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Checkin checkin)
        {
            _context.Checkins.Update(checkin);
            await _context.SaveChangesAsync();
        }

        public async Task<Checkin> SoftDeleteAsync(Guid id)
        {
            var checkin = await _context.Checkins.FindAsync(id);
            if (checkin == null) return null;
            checkin.IsDeleted = true;
            await _context.SaveChangesAsync();
            return checkin;
        }
        public async Task<bool> ExistsAsync(Expression<Func<Checkin, bool>> predicate)
        {
            return await _context.Checkins.AnyAsync(predicate);
        }
    }
}
