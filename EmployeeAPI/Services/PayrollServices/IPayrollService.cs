using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Payrolls;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;

namespace EmployeeAPI.Services.PayrollServices
{
     public interface IPayrollService
    {
        Task<ResponseModel.PayrollDto> GetPayrollById(Guid id);
        Task<IEnumerable<ResponseModel.PayrollDto>> GetAllPayrolls();
        Task<ResponseModel.CreatePayroll> CreatePayroll(ResponseModel.CreatePayroll dto);
        Task<ResponseModel.PayrollDto> UpdatePayroll(ResponseModel.UpdatePayroll dto);
        Task<string> SoftDeletePayroll(Guid id);
        Task<IEnumerable<ResponseModel.PayrollDto>> GetCheckinsByStaffAndMonthAsync(Guid staffId, int year, int month);
        ///////////////////////////////
        Task<PaidPayroll> CalculatePayrollAsync(Guid staffId);
    }
}
