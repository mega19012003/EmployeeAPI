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
            return await _context.Payrolls.AnyAsync(p => p.UserId == UserId && p.CreatedDate.Month == month && p.CreatedDate.Year == year && !p.IsDeleted);
        }

        private async Task<int> CountCheckinsByStatus(Guid userId, CheckinStatus status, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.UserId == userId && c.CheckinStatus == status && c.CheckinDate.Month == month && c.CheckinDate.Year == year && !c.IsDeleted);
        }

        public Task<int> CountValidCheckins(Guid userId, int month, int year)
        {
            return CountCheckinsByStatus(userId, CheckinStatus.OnTime, month, year);
        }

        public Task<int> CountLateCheckins(Guid userId, int month, int year)
        {
            return CountCheckinsByStatus(userId, CheckinStatus.Late, month, year);
        }

        public Task<int> CountLeaveEarlyCheckins(Guid userId, int month, int year)
        {
            return CountCheckinsByStatus(userId, CheckinStatus.LeaveEarly, month, year);
        }

        public Task<int> CountAbsentCheckins(Guid userId, int month, int year)
        {
            return CountCheckinsByStatus(userId, CheckinStatus.Absent, month, year);
        }

        public Task<int> CountAbsentPermissionCheckins(Guid UserId, int month, int year)
        {
            return CountCheckinsByStatus(UserId, CheckinStatus.LeaveWithPermission, month, year);
        }

        public Task<int> CountOvertimeCheckins(Guid UserId, int month, int year)
        {
            return CountCheckinsByStatus(UserId, CheckinStatus.Overtime, month, year);
        }

        public Task<int> CountothersCheckins(Guid UserId, int month, int year)
        {
            return CountCheckinsByStatus(UserId, CheckinStatus.Overtime, month, year);
        }

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
