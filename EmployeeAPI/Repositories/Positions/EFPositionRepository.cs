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
            return await _context.Positions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
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
                .Include(p => p.Users.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Name.ToLower().Equals(name.ToLower()));
        }
        public async Task<IEnumerable<Position>> GetStaffByPositionAsync(Guid positionId, int? pageSize, int? pageIndex)
        {
           /* var query = _context.Positions
                .AsNoTracking()
                .Include(s => s.Users)
                .Where(s => !s.IsDeleted && s.Id == positionId);

            if (pageSize.HasValue && pageIndex.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }*/

            return await _context.Positions.AsNoTracking().ToListAsync();
        }

    }
}
