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
            var result = _context.Payrolls
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                result = result.Where(p => p.Users.Fullname.Contains(name));
            }
            if (pageSize.HasValue && pageIndex.HasValue)
            {
                result = result.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await result.Include(p => p.Users).Where(p => p.Users.IsDeleted == false && p.IsDeleted == false).ToListAsync(); 
        }

        public async Task<Payroll> GetPayrollById(Guid id)
        {
            return await _context.Payrolls
                .AsNoTracking()
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /*public async Task CreatePayroll(Payroll payroll)
        {
            var result = _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();
        }*/

        public async Task UpdatePayroll(Payroll payroll)
        {
            _context.Payrolls.Update(payroll);
            await _context.SaveChangesAsync();
        }

        public async Task<Payroll> SoftDeletePayroll(Guid id)
        {
            var entity = await _context.Payrolls.FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);
            if(entity == null)  return null;

            entity.IsDeleted = true;
            _context.Payrolls.Update(entity);
            await _context.SaveChangesAsync();
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
            return await _context.Payrolls.AnyAsync(p => p.UserId == UserId &&
                                                         p.CreatedDate.Month == month &&
                                                         p.CreatedDate.Year == year &&
                                                         !p.IsDeleted);
        }

        public async Task<int> CountValidCheckins(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.Status == CheckinStatus.OnTime &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountLateCheckins(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.Status == CheckinStatus.Late &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountAbsentCheckins(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.Status == CheckinStatus.Absent &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountAbsentPermissionCheckins(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.Status == CheckinStatus.LeaveWithPermission &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountLeaveEarlyCheckins(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.Status == CheckinStatus.LeaveEarly &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountOvertimeCheckins(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.Status == CheckinStatus.Overtime &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        /*public async Task<int> CountOnHolidayPermissionCheckins(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.Status == CheckinStatus.OnHoliday &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }*/

        public async Task<User> GetUserWithSalary(Guid UserId)
        {
            return await _context.Users.FirstOrDefaultAsync(s => s.UserId == UserId && !s.IsDeleted);
        }

        public async Task CreatePayrollAsync(Payroll payroll)
        {
            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountDayWorked(Guid UserId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == UserId &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }
    }
}
