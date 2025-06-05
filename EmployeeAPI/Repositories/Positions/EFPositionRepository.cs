using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Positions
{
    public class EFPositionRepository : IPositionRepository
    {
        private readonly AppDbContext _context;
        public EFPositionRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Position> GetQueryable()
        {
            return _context.Positions.AsNoTracking().Where(p => !p.IsDeleted);
        }

        public async Task<Position?> GetByIdAsync(Guid id)
        {
            return await _context.Positions.AsNoTracking().Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Position> AddAsync(Position position)
        {
            await _context.Positions.AddAsync(position);
            //await _context.SaveChangesAsync();
            return position;
        }

        public async Task<Position?> UpdateAsync(Position position)
        {
            var entity = await _context.Positions.FirstOrDefaultAsync(p => p.Id == position.Id && !p.IsDeleted);
            if (entity == null) return null;

            entity.Name = position.Name;
            //_context.Positions.Update(entity);
            //await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Position> SoftDeleteAsync(Guid id)
        {
            var entity = await _context.Positions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (entity == null) return null;
            entity.IsDeleted = true;
            //_context.Positions.Update(entity);
            //await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Position?> GetAllEmployee(string name)
        {
            return await _context.Positions
                .AsNoTracking()
                .Include(p => p.Users.Where(s => s.IsActive ))
                .FirstOrDefaultAsync(p => p.Name.ToLower().Equals(name.ToLower()));
        }
        
        public async Task<IEnumerable<Position>> GetStaffByPositionAsync(Guid positionId, int? pageSize, int? pageIndex)
        {
            return await _context.Positions
                .AsNoTracking()
                .Include(d => d.Users.Where(u => u.IsActive && !u.IsDeleted))
                .ThenInclude(u => u.Department)
                .Where(d => !d.IsDeleted && d.Id == positionId)
                .ToListAsync();
        }

    }
}
