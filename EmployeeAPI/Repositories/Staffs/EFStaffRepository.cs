using EmployeeAPI.Models;
using EmployeeAPI.Services.FileServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Staffs
{
    public class EFStaffRepository : IStaffRepository
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        public EFStaffRepository(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IEnumerable<Staff>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm)
        {
            var results = _context.Staffs.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                results = results.Where(f => f.Name.Contains(SearchTerm));
            }
            if (pageSize.HasValue && pageIndex.HasValue)
            {
                results = results.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return await results.Where(p => !p.IsDeleted && p.IsActive).ToListAsync();
        }


        public async Task<Staff> GetByIdAsync(Guid id)
        {
            var results = await _context.Staffs.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && p.IsActive);
            return results;
        }

        public async Task<Staff> AddAsync(Staff staff)
        {
            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }


        public async Task<Staff> UpdateAsync(Staff staff)
        {
            var existingStaff = await _context.Staffs.FirstOrDefaultAsync(p => p.Id == staff.Id && !p.IsDeleted && p.IsActive);

            if (existingStaff == null)
                return null; 
            existingStaff.Name = staff.Name;
            existingStaff.DepartmentId = staff.DepartmentId;
            existingStaff.PositionId = staff.PositionId;
            existingStaff.BasicSalary = staff.BasicSalary;
            existingStaff.ImageUrl = staff.ImageUrl;
            existingStaff.IsActive = staff.IsActive;

            await _context.SaveChangesAsync();
            return existingStaff;
        }

        public async Task<Staff> SoftDeleteAsync(Guid Id)
        {

            var existingStaff = await _context.Staffs.FirstOrDefaultAsync(p => p.Id == Id && !p.IsDeleted && p.IsActive);

            if (existingStaff == null)
                return null;

            existingStaff.IsDeleted = true;
            _context.Staffs.Update(existingStaff);
            await _context.SaveChangesAsync();
            return existingStaff;
        }


        public async Task<IEnumerable<Staff>> GetByNameAsync(string name, int? pageSize, int? pageIndex)
        {
            var query = _context.Staffs.Where(p => p.IsActive == true && !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(f => f.Name.ToLower().Contains(name.ToLower()));
            }

            if (pageSize.HasValue && pageIndex.HasValue)
            {
                int skip = (pageIndex.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Staff>> GetEmployeeByPosition(string SearchTerm, int? pageSize, int? pageIndex)
        {
            var query = _context.Staffs
                .Include(s => s.Position)
                .Where(s => !s.IsDeleted);

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(f => f.Position.Name.ToLower().Contains(SearchTerm.ToLower()));
            }

            return await query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToListAsync();
        }
    }
}
