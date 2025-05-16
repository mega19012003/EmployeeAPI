using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Payrolls
{
    public interface IPayrollRepository
    {
        Task<Payroll> GetPayrollById(Guid id);
        Task<IEnumerable<Payroll>> GetAllPayrolls();
        Task CreatePayroll(Payroll payroll);
        Task UpdatePayroll(Payroll payroll);
        Task<Payroll> SoftDeletePayroll(Guid id);
        Task<IEnumerable<Payroll>> GetCheckinsByStaffAndMonth(Guid staffId);
        /// <summary>
        /// ////////////////////////////////////////////////
        /// </summary>
        Task<bool> ExistsPayrollForMonth(Guid staffId, int month, int year);
        Task<int> CountValidCheckins(Guid staffId, int month, int year);
        Task<int> CountLateCheckins(Guid staffId, int month, int year);
        Task<int> CountAbsentCheckins(Guid staffId, int month, int year);
        Task<int> CountAbsentPermissionCheckins(Guid staffId, int month, int year);
        Task<int> CountLeaveEarlyCheckins(Guid staffId, int month, int year);
        Task<int> CountOvertimeCheckins(Guid staffId, int month, int year);
        Task<int> CountOnHolidayPermissionCheckins(Guid staffId, int month, int year);
        Task<Staff> GetStaffWithSalary(Guid staffId);
        Task CreatePayrollAsync(Payroll payroll);
    }
}
