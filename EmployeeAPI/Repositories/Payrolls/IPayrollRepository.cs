using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Payrolls
{
    public interface IPayrollRepository
    {
        Task<Payroll> GetPayrollById(Guid id);
        Task<IEnumerable<Payroll>> GetAllPayrolls(string? name, int? pageIndex, int? pageSize);
        Task UpdatePayroll(Payroll payroll);
        Task<Payroll> SoftDeletePayroll(Guid id);
        Task<IEnumerable<Payroll>> GetPayrollByUserAsync(Guid userId, int? pageIndex, int? pageSize);
        /// <summary>
        /// ////////////////////////////////////////////////
        /// </summary>
        Task<bool> ExistsPayrollForMonth(Guid userId, int month, int year);
        Task<int> CountValidCheckins(Guid userId, int month, int year);
        Task<int> CountLateCheckins(Guid userId, int month, int year);
        Task<int> CountAbsentCheckins(Guid userId, int month, int year);
        Task<int> CountAbsentPermissionCheckins(Guid userId, int month, int year);
        Task<int> CountLeaveEarlyCheckins(Guid userId, int month, int year);
        Task<int> CountOvertimeCheckins(Guid userId, int month, int year);
        //Task<int> CountOnHolidayPermissionCheckins(Guid userId, int month, int year);
        Task<int> CountDayWorked(Guid userId, int month, int year);
        Task<User> GetUserWithSalary(Guid userId);
        Task CreatePayrollAsync(Payroll payroll);
    }
}
