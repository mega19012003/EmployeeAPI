using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Payrolls;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;

namespace EmployeeAPI.Services.PayrollServices
{
     public interface IPayrollService
    {
        Task<PagedResult<ResponseModel.PayrollResultDto>> GetAllPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize);
        Task<ResponseModel.PayrollResultDto> GetById(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeletePayroll(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        //Task<IEnumerable<ResponseModel.PayrollResultDto>> GetCheckinsByStaffAndMonthAsync(Guid staffId, int year, int month);
        ///////////////////////////////
        //Task<PaidPayroll> CalculatePayrollAsync(Guid staffId);
        Task<PaidPayrollDto> CalculatePayrollAsync(Guid staffId, Guid currentUserId, IList<string> currentUserRoles);
        Task<PagedResult<ResponseModel.PayrollResultDto>> GetPayrollByUser(Guid? staffId, Guid currentUserId, IList<string> currentRoles, int? pageIndex, int? pageSize);
    }
}
