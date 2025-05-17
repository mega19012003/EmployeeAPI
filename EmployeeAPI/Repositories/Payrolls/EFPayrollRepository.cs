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

        public async Task<IEnumerable<Payroll>> GetAllPayrolls()
        {
            var result = await _context.Payrolls
                .Include(p => p.Staff)
                .Where(p => p.Staff.IsDeleted == false && p.IsDeleted == false)
                .ToListAsync();
            return result;
        }

        public async Task<Payroll> GetPayrollById(Guid id)
        {
            return await _context.Payrolls
                .Include(p => p.Staff)
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

        public async Task<IEnumerable<Payroll>> GetPayrollByStaffAsync(Guid id)
        {
            var result = await _context.Payrolls
                .Include(p => p.Staff)
                .Where(p => p.StaffId == id && p.IsDeleted == false)
                .ToListAsync();
            return result;
        }

        /// <summary>
        /// ////////////////////////////////////////////////
        /// </summary>

        public async Task<bool> ExistsPayrollForMonth(Guid staffId, int month, int year)
        {
            return await _context.Payrolls.AnyAsync(p => p.StaffId == staffId &&
                                                         p.CreatedDate.Month == month &&
                                                         p.CreatedDate.Year == year &&
                                                         !p.IsDeleted);
        }

        public async Task<int> CountValidCheckins(Guid staffId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.StaffId == staffId &&
                                                           c.Status == CheckinStatus.OnTime &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountLateCheckins(Guid staffId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.StaffId == staffId &&
                                                           c.Status == CheckinStatus.Late &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountAbsentCheckins(Guid staffId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.StaffId == staffId &&
                                                           c.Status == CheckinStatus.Absent &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountAbsentPermissionCheckins(Guid staffId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.StaffId == staffId &&
                                                           c.Status == CheckinStatus.AbsentWithPermission &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountLeaveEarlyCheckins(Guid staffId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.StaffId == staffId &&
                                                           c.Status == CheckinStatus.LeaveEarly &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountOvertimeCheckins(Guid staffId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.StaffId == staffId &&
                                                           c.Status == CheckinStatus.Overtime &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<int> CountOnHolidayPermissionCheckins(Guid staffId, int month, int year)
        {
            return await _context.Checkins.CountAsync(c => c.StaffId == staffId &&
                                                           c.Status == CheckinStatus.OnHoliday &&
                                                           c.CheckinDate.Month == month &&
                                                           c.CheckinDate.Year == year &&
                                                           c.IsDeleted == false);
        }

        public async Task<Staff> GetStaffWithSalary(Guid staffId)
        {
            return await _context.Staffs.FirstOrDefaultAsync(s => s.Id == staffId && !s.IsDeleted);
        }

        public async Task CreatePayrollAsync(Payroll payroll)
        {
            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();
        }
    }
}
