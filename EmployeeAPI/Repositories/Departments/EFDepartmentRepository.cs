using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Staffs;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Departments
{
    public class EFDepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;
        private readonly IStaffRepository _staffRepository;
        public EFDepartmentRepository(AppDbContext context, IStaffRepository staffRepository)
        {
            _context = context;
            _staffRepository = staffRepository;
        }
        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments.Where(p => !p.isDeleted).ToListAsync();
        }
        public async Task<Department> GetByIdAsync(Guid id)
        {
            return await _context.Departments.FirstOrDefaultAsync(p => p.Id == id && !p.isDeleted);
        }
        public async Task<Department> AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            return department;

        }

        public async Task<Department> UpdateAsync(Department department)
        {
            var results = await _context.Departments.FirstOrDefaultAsync(p => p.Id == department.Id && !p.isDeleted);

            if (results == null) return null;

            results.Name = department.Name;
            await _context.SaveChangesAsync();
            return results;
        }

        public async Task<Department> SoftDeleteAsync(Guid id)
        {
            var results = await _context.Departments.FirstOrDefaultAsync(p => p.Id == id && !p.isDeleted);
            if (results == null) return null;

            results.isDeleted = true;
            _context.Departments.Update(results);
            await _context.SaveChangesAsync();
            return results;
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

        public async Task<Department> GetAllEmployee(string name)
        {
            var results = await _context.Departments
                .Include(d => d.Staffs)
                .FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower() && !d.isDeleted);
            return results;
        }
    }
}
