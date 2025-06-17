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
        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments.Where(p => p.isDeleted).AsNoTracking().ToListAsync();
        }

        public async Task<Department> GetByIdAsync(Guid id)
        {
            return await _context.Departments.Include(d => d.Positions).FirstOrDefaultAsync(p => p.Id == id && !p.isDeleted);
        }
        public async Task AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
        }

        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var results = await _context.Departments.FirstOrDefaultAsync(p => p.Id == id && !p.isDeleted);
            if (results == null) return;

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

        public async Task<IEnumerable<Department>> GetStaffByDepartmentAsync()
        {
            return await _context.Departments.Include(p => p.Users.Where(u => !u.IsDeleted && u.IsActive)).Where(p => !p.isDeleted).ToListAsync();
        }

        public async Task<IEnumerable<Department>> GetPositionsByDepartmentAsync(Guid? id)
        {
            return await _context.Departments.Where(p => !p.isDeleted && p.Id == id).Include(p => p.Positions.Where(p => !p.IsDeleted)).AsNoTracking().ToListAsync();
        }
    }
}
