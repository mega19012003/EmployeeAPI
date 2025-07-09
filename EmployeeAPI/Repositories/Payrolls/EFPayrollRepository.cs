using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Payrolls
{
    public class EFPayrollRepository : IPayrollRepository
    {
        private readonly AppDbContext _context;

        public EFPayrollRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payroll>> GetAllPayrolls(string? name, int? pageIndex, int? pageSize)
        {
            return await _context.Payrolls.Include(p => p.Users).Where(p => p.Users.IsDeleted == false && p.IsDeleted == false).AsNoTracking().ToListAsync(); 
        }

        public async Task<Payroll> GetPayrollById(Guid id)
        {
            return await _context.Payrolls
                .AsNoTracking()
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Payroll> SoftDeletePayroll(Guid id)
        {
            var entity = await _context.Payrolls.FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);
            if(entity == null)  return null;

            return entity;
        }

        public async Task<IEnumerable<Payroll>> GetPayrollByUserAsync(Guid id, int? pageIndex, int? pageSize)
        {
            var result = await _context.Payrolls
                .AsNoTracking()
                .Include(p => p.Users)
                .Where(p => p.UserId == id && p.IsDeleted == false)
                .ToListAsync();
            return result;
        }

        /// <summary>
        /// ////////////////////////////////////////////////
        /// </summary>
        public async Task<bool> ExistsPayrollForMonth(Guid UserId, int month, int year)
        {
            return await _context.Payrolls.AnyAsync(p => p.UserId == UserId && p.CreatedDate.Month == month && p.CreatedDate.Year == year && !p.IsDeleted);
        }

        public async Task<User> GetUserWithSalary(Guid UserId)
        {
            return await _context.Users.FirstOrDefaultAsync(s => s.UserId == UserId && !s.IsDeleted);
        }

        public async Task CreatePayrollAsync(Payroll payroll)
        {
            _context.Payrolls.Add(payroll);
            //await _context.SaveChangesAsync();
        }
        public async Task<int> CountDayWorked(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.CheckinTime.Month == month &&
                                                           c.CheckinTime.Year == year &&
                                                           c.IsDeleted == false);
        }
    }
}
