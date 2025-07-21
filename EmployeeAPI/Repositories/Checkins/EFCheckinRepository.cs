using System.Linq.Expressions;
using EmployeeAPI.Base;
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
            return _context.Checkins
                .AsNoTracking()
                .Include(c => c.Users)
                .Where(c => !c.IsDeleted);
        }

        public IQueryable<Checkin> GetAll()
        {
            return _context.Checkins
                .AsNoTracking()
                .Include(c => c.Users)
                .Where(c => !c.IsDeleted);
        }

        public async Task<Checkin> GetByIdAsync(Guid id)
        {
            return await _context.Checkins
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && !c.Users.IsDeleted);
        }

        public async Task CreateAsync(Checkin checkin)
        {
            _context.Checkins.Add(checkin);

        }

        public async Task UpdateAsync(Checkin checkin)
        {
            _context.Checkins.Update(checkin);
        }

        public async Task<Checkin> SoftDeleteAsync(Guid id)
        {
            var checkin = await _context.Checkins.FindAsync(id);
            if (checkin == null) return null;
            checkin.IsDeleted = true;
            return checkin;
        }


        public async Task<IEnumerable<Checkin>> GetCheckinByUserAsync(Guid UserId, int? pageIndex, int? pageSize)
        {
            var query = _context.Checkins
                 .AsNoTracking()
                 .Where(s => !s.IsDeleted);

            if (pageSize.HasValue && pageIndex.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> ExistAsync(Guid UserId)
        {
            return await _context.Checkins.AnyAsync(c => c.UserId == UserId && !c.IsDeleted);
        }
    }
}
