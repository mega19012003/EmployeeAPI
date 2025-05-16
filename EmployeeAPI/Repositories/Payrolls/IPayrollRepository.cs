using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Payrolls
{
    public interface IPayrollRepository
    {
        Task<Payroll> GetPayrollById(Guid id);
        Task<IEnumerable<Payroll>> GetAllPayrolls();
        Task<Payroll> CreatePayroll(Payroll payroll);
        Task<Payroll> UpdatePayroll(Payroll payroll);
        Task<bool> DeletePayroll(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
