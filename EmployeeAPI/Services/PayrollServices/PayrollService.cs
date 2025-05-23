using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Payrolls;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;

namespace EmployeeAPI.Services.PayrollServices
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICheckinRepository _checkinRepository;
        private readonly AppDbContext _context;
        public PayrollService(IPayrollRepository payrollRepository, ICheckinRepository checkinRepository, AppDbContext context)
        {
            _payrollRepository = payrollRepository;
            _checkinRepository = checkinRepository;
            _context = context;
        }

        public async Task<PagedResult<ResponseModel.PayrollDto>> GetAllPayrolls(string? name, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Payrolls
                .Include(c => c.Users)
                .Where(f => string.IsNullOrEmpty(name) || f.Users.Fullname.ToLower().Contains(name.ToLower()))
                .Where(p => !p.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new ResponseModel.PayrollDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Name = c.Users.Fullname,
                    Salary = c.Salary,
                    DaysWorked = c.DaysWorked,
                    CreatedDate = c.CreatedDate,
                    Note = c.Note,
                    IsDeleted = c.IsDeleted,
                }).ToListAsync();

            return new PagedResult<ResponseModel.PayrollDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }

        public async Task<ResponseModel.PayrollDto> GetPayrollById(Guid id)
        {
            var result = await _payrollRepository.GetPayrollById(id);
            if (result == null)
            {
                return null;
            }
            return new PayrollDto
            {
                Id = result.Id,
                UserId = result.UserId,
                CreatedDate = result.CreatedDate,
                Note = result.Note,
            };

        }

        /*public async Task<ResponseModel.PayrollDto> UpdatePayroll(ResponseModel.UpdatePayroll dto)
        {
            var exsistingPayroll = await _payrollRepository.GetPayrollById(dto.Id);
            if (exsistingPayroll == null)
            {
                return null;
            }
            
            exsistingPayroll.UserId = dto.UserId;
            exsistingPayroll.Note = dto.Note;

            await _payrollRepository.UpdatePayroll(exsistingPayroll);
            return new ResponseModel.PayrollDto
            {
                Id = exsistingPayroll.Id,
                UserId = exsistingPayroll.UserId,
                CreatedDate = exsistingPayroll.CreatedDate,
                Note = exsistingPayroll.Note,
            };
        }*/

        public async Task<string> SoftDeletePayroll(Guid id)
        {
            var result = await _payrollRepository.SoftDeletePayroll(id);
            if (result == null) return null;
            /*result.IsDeleted = true;
            await _payrollRepository.UpdatePayroll(result);*/
            return "Đã xóa payroll " + id;
        }

        public async Task<PagedResult<ResponseModel.PayrollDto>> GetPayrollByUser(Guid UserId, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Payrolls
                .Include(c => c.Users)
                .Where(p => !p.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new ResponseModel.PayrollDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Name = c.Users.Fullname,
                    Salary = c.Salary,
                    CreatedDate = c.CreatedDate,
                    Note = c.Note,
                    IsDeleted = c.IsDeleted,
                }).ToListAsync();

            return new PagedResult<ResponseModel.PayrollDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }

        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        
        public async Task<PaidPayroll> CalculatePayrollAsync(Guid UserId)
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            if (await _payrollRepository.ExistsPayrollForMonth(UserId, month, year))
                throw new InvalidOperationException("Payroll for this month already existed");

            var User = await _payrollRepository.GetUserWithSalary(UserId);
            if (User == null) throw new Exception("Cannot find User id");

            var validCheckins = await _payrollRepository.CountValidCheckins(UserId, month, year);
            var lateCheckins = await _payrollRepository.CountLateCheckins(UserId, month, year);
            var absentCheckins = await _payrollRepository.CountAbsentCheckins(UserId, month, year);
            var absentPermissionCheckins = await _payrollRepository.CountAbsentPermissionCheckins(UserId, month, year);
            var leaveEarlyCheckins = await _payrollRepository.CountLeaveEarlyCheckins(UserId, month, year);
            var overtimeCheckins = await _payrollRepository.CountOvertimeCheckins(UserId, month, year);
            //var onHolidayPermissionCheckins = await _payrollRepository.CountOnHolidayPermissionCheckins(UserId, month, year);

            var basic = User.BasicSalary;
            var bonus30 = basic * 1.3;
            var bonus50 = basic * 1.5;
            var penalty10 = basic * 0.9;
            var penalty30 = basic * 0.7;
            var penalty50 = basic * 0.5;

            var totalSalary = basic * validCheckins 
                                + (bonus30 * overtimeCheckins)
                                //+ (bonus50 * onHolidayPermissionCheckins)
                                + (penalty30 * lateCheckins)
                                + (penalty30 * leaveEarlyCheckins)
                                + (penalty10 * absentPermissionCheckins)
                                + (penalty50 * absentCheckins);

            var totalDayWorked = await _payrollRepository.CountDayWorked(UserId, month, year);

            var payroll = new Payroll
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                Salary = totalSalary,
                DaysWorked = totalDayWorked,
                CreatedDate = DateTime.Now,
                Note = $"Lương tháng {month}/{year}",
            };

            await _payrollRepository.CreatePayrollAsync(payroll);

            return new PaidPayroll
            {
                Id = payroll.Id,
                UserId = UserId,
                DaysWorked = totalDayWorked,
                Salary = totalSalary,
                CreatedDate = payroll.CreatedDate,
                Note = payroll.Note
            };
        }
    }
}
