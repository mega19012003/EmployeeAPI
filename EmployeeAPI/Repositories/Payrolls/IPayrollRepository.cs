using EmployeeAPI.Enums;
using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Payrolls
{
    public interface IPayrollRepository
    {
        Task<Payroll> GetPayrollById(Guid id);
        Task<IEnumerable<Payroll>> GetAllPayrolls(string? name, int? pageIndex, int? pageSize);
        //Task UpdatePayroll(Payroll payroll);
        Task<Payroll> SoftDeletePayroll(Guid id);
        Task<IEnumerable<Payroll>> GetPayrollByUserAsync(Guid userId, int? pageIndex, int? pageSize);

        /// ////////////////////////////////////////////////


        //Task<int> CountCheckinsByStatus(Guid userId, CheckinMorningStatus status, int month, int year);
        Task<bool> ExistsPayrollForMonth(Guid userId, int month, int year);
        //Task<User> GetUserWithSalary(Guid userId);
        Task CreatePayrollAsync(Payroll payroll);
    }
}
