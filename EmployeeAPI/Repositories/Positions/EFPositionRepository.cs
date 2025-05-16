using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Staffs;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Positions
{
    public class EFPositionRepository : IPositionRepository
    {
        private readonly AppDbContext _context;
        private readonly IStaffRepository _staffRepository;
        public EFPositionRepository(AppDbContext context, IStaffRepository staffRepository)
        {
            _context = context;
            _staffRepository = staffRepository;
        }

        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            return await _context.Positions.Where(p => !p.IsDeleted).ToListAsync();
        }

        public async Task<Position?> GetByIdAsync(Guid id)
        {
            return await _context.Positions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Position> AddAsync(Position position)
        {
            await _context.Positions.AddAsync(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<Position?> UpdateAsync(Position position)
        {
            var entity = await _context.Positions.FirstOrDefaultAsync(p => p.Id == position.Id && !p.IsDeleted);
            if (entity == null) return null;

            entity.Name = position.Name;
            //_context.Positions.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _context.Positions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (entity == null) return;

            _context.Positions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Position?> GetAllEmployee(string name)
        {
            return await _context.Positions
                .Include(p => p.Staffs.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Name.ToLower().Equals(name.ToLower()));
        }
        public async Task<IEnumerable<Position>> GetStaffByPositionAsync(string positionName, int? pageSize, int? pageIndex)
        {
            var query = _context.Positions
                .Include(s => s.Staffs)
                .Where(s => !s.IsDeleted);

            if (!string.IsNullOrEmpty(positionName))
            {
                query = query.Where(s => s.Name.ToLower().Contains(positionName.ToLower()));
            }

            if (pageSize.HasValue && pageIndex.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query.ToListAsync();
        }

    }
}
