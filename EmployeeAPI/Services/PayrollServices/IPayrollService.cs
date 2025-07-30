using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Payrolls;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;

namespace EmployeeAPI.Services.PayrollServices
{
     public interface IPayrollService
    {
        Task<PagedResult<ResponseModel.PayrollResultDto>> GetAllPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, Guid? companyId, int? Month, int? Year, int? pageIndex, int? pageSize);
        Task<ResponseModel.PayrollResultDto> GetById(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeletePayroll(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        ///////////////////////////////
        //Task<PaidPayroll> CalculatePayrollAsync(Guid staffId);
        //Task<PayrollResultDto> CalculatePayrollAsync(Guid staffId, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.PayrollResultDto> CalculatePayrollAsync(Guid staffId, int Month, int Year, Guid currentUserId, IList<string> currentUserRoles);
        //Task<List<ResponseModel.PayrollResultDto>> CalculatePayrollForAllUsersAsync(Guid currentUserId, IList<string> currentUserRoles);
        //Task<PagedResult<ResponseModel.PayrollResultDto>> GetPayrollByUser(Guid? staffId, Guid currentUserId, IList<string> currentRoles, int? pageIndex, int? pageSize);
        Task<PagedResult<ResponseModel.UserWithPayrollDto>> GetUsersWithPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, Guid? companyId, Guid? departmentId, Guid? positionId, int? month, int? year, int? pageIndex, int? pageSize);
    }
}
