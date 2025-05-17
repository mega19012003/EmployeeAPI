using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Payrolls;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;

namespace EmployeeAPI.Services.PayrollServices
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICheckinRepository _checkinRepository;
        public PayrollService(IPayrollRepository payrollRepository, ICheckinRepository checkinRepository)
        {
            _payrollRepository = payrollRepository;
            _checkinRepository = checkinRepository;
        }

        public async Task<IEnumerable<ResponseModel.PayrollDto>> GetAllPayrolls()
        {
            var result = await _payrollRepository.GetAllPayrolls();
            return result.Select(c => new PayrollDto
            {
                Id = c.Id,
                StaffId = c.StaffId,
                StaffName = c.Staff.Name,
                Salary = c.Salary,
                CreatedDate = c.CreatedDate,
                Note = c.Note,
                IsDeleted = c.IsDeleted,
            });
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
                StaffId = result.StaffId,
                CreatedDate = result.CreatedDate,
                Note = result.Note,
            };

        }

        public async Task<ResponseModel.PayrollDto> UpdatePayroll(ResponseModel.UpdatePayroll dto)
        {
            var exsistingPayroll = await _payrollRepository.GetPayrollById(dto.Id);
            if (exsistingPayroll == null)
            {
                return null;
            }
            
            exsistingPayroll.StaffId = dto.StaffId;
            exsistingPayroll.Note = dto.Note;

            await _payrollRepository.UpdatePayroll(exsistingPayroll);
            return new ResponseModel.PayrollDto
            {
                Id = exsistingPayroll.Id,
                StaffId = exsistingPayroll.StaffId,
                CreatedDate = exsistingPayroll.CreatedDate,
                Note = exsistingPayroll.Note,
            };
        }

        public async Task<string> SoftDeletePayroll(Guid id)
        {
            var result = await _payrollRepository.SoftDeletePayroll(id);
            if (result == null) return null;
            /*result.IsDeleted = true;
            await _payrollRepository.UpdatePayroll(result);*/
            return "Đã xóa payroll";
        }

        public async Task<IEnumerable<ResponseModel.PayrollDto>> GetPayrollByStaff(Guid staffId)
        {
            var result = await _payrollRepository.GetPayrollByStaffAsync(staffId);
            return result.Select(c => new PayrollDto
            {
                Id = c.Id,
                StaffId = c.StaffId,
                StaffName = c.Staff.Name,
                Salary = c.Salary,
                CreatedDate = c.CreatedDate,
                Note = c.Note,
                IsDeleted = c.IsDeleted,
            });
        }

        ////////////////////////////////////////////////////////
        ///
        public async Task<PaidPayroll> CalculatePayrollAsync(Guid staffId)
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            if (await _payrollRepository.ExistsPayrollForMonth(staffId, month, year))
                throw new InvalidOperationException("Đã tồn tại bảng lương cho tháng này.");

            var staff = await _payrollRepository.GetStaffWithSalary(staffId);
            if (staff == null) throw new Exception("Nhân viên không tồn tại.");

            var validCheckins = await _payrollRepository.CountValidCheckins(staffId, month, year);
            var lateCheckins = await _payrollRepository.CountLateCheckins(staffId, month, year);
            var absentCheckins = await _payrollRepository.CountAbsentCheckins(staffId, month, year);
            var absentPermissionCheckins = await _payrollRepository.CountAbsentPermissionCheckins(staffId, month, year);
            var leaveEarlyCheckins = await _payrollRepository.CountLeaveEarlyCheckins(staffId, month, year);
            var overtimeCheckins = await _payrollRepository.CountOvertimeCheckins(staffId, month, year);
            var onHolidayPermissionCheckins = await _payrollRepository.CountOnHolidayPermissionCheckins(staffId, month, year);

            var basic = staff.BasicSalary;
            var bonus30 = basic * 1.3;
            var bonus50 = basic * 1.5;
            var penalty10 = basic * 0.9;
            var penalty30 = basic * 0.7;
            var penalty50 = basic * 0.5;

            var totalSalary = basic * validCheckins 
                                + (bonus30 * overtimeCheckins)
                                + (bonus50 * onHolidayPermissionCheckins)
                                + (penalty30 * lateCheckins)
                                + (penalty30 * leaveEarlyCheckins)
                                + (penalty10 * absentPermissionCheckins)
                                + (penalty50 * absentCheckins);

            var payroll = new Payroll
            {
                Id = Guid.NewGuid(),
                StaffId = staffId,
                Salary = totalSalary,
                CreatedDate = DateTime.Now,
                Note = $"Lương tháng {month}/{year}",
                IsPaid = false
            };

            await _payrollRepository.CreatePayrollAsync(payroll);

            return new PaidPayroll
            {
                Id = payroll.Id,
                StaffId = staffId,
                
                //BasicSalary = staff.BasicSalary,
                //ValidCheckinCount = validCheckins,
                Salary = totalSalary,
                CreatedDate = payroll.CreatedDate,
                Note = payroll.Note
            };
        }
    }
}
