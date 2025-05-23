using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Departments
{
    public class EFDepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;
        private readonly IAuthRepository _authRepository;
        public EFDepartmentRepository(AppDbContext context, IAuthRepository authRepository)
        {
            _context = context;
            _authRepository = authRepository;
        }
        public async Task<IEnumerable<Department>> GetAllAsync(string? name, int? pageIndex, int? pageSize)
        {
            var results = _context.Departments
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                results = results.Where(f => f.Name.Contains(name));
            }
            if (pageSize.HasValue && pageIndex.HasValue)
            {
                results = results.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return await results.Where(p => !p.isDeleted).ToListAsync();
        }

        public async Task<Department> GetByIdAsync(Guid id)
        {
            return await _context.Departments.FirstOrDefaultAsync(p => p.Id == id && !p.isDeleted);
        }
        public async Task AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            //await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            /*var existingDepartment = await _context.Departments.FirstOrDefaultAsync(p => p.Id == department.Id && !p.isDeleted);

            if (existingDepartment == null) return;*/

            //existingDepartment.Name = department.Name;
            _context.Departments.Update(department);
            //await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var results = await _context.Departments.FirstOrDefaultAsync(p => p.Id == id && !p.isDeleted);
            if (results == null) return;

            //results.isDeleted = true;
            _context.Departments.Update(results);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Department>> GetDepartmentByName(string name)
        {
            var results = await _context.Departments
                .Where(d => d.Name.ToLower().Contains(name.ToLower()) && !d.isDeleted)
                .ToListAsync();
            if (results == null)
            {
                return null;
            }
            return results;
        }
        public async Task<IEnumerable<Department>> GetStaffByDepartmentAsync(string positionName, int? pageSize, int? pageIndex)
        {
            var query = _context.Departments
                .AsNoTracking()
                .Include(s => s.Users)
                .Where(s => !s.isDeleted);

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

        public async Task<IEnumerable<Department>> GetPositionsByDepartmentAsync(Guid id, int? pageSize, int? pageIndex)
        {
            var query = _context.Departments
                .AsNoTracking()
                .Include(s => s.Positions)
                .Where(s => !s.isDeleted);

            /*if (!string.IsNullOrEmpty(positionName))
            {
                query = query.Where(s => s.Name.ToLower().Contains(positionName.ToLower()));
            }

            if (pageSize.HasValue && pageIndex.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }*/

            //return await query.ToListAsync();
            return await _context.Departments.AsNoTracking().ToListAsync();
        }
    }
}
