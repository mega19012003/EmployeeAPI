using System.Linq.Expressions;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;


namespace EmployeeAPI.Services.CheckinServices
{
    public interface ICheckinService
    {
        Task<PagedResult<ResponseModel.CheckinResultDto>> GetAllAsync(string? StaffName, Guid? companyId, Guid? departmentId, Guid? positionId, int? day, int? month, int? year, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
        //Task AutoMarkAbsentAsync(TimeOnly EndTimeAfternoon);
        Task<ResponseModel.CheckinResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.CheckinResultDto> CheckinAsync(Guid? userId, Guid currentUserId, IList<string> roles);
        Task<ResponseModel.CheckinResultDto> CheckoutAsync(Guid? userId, Guid currentUserId, IList<string> roles);
        Task<ResponseModel.CheckinResultDto> UpdateAsync(ResponseModel.UpdateCheckinDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        //Task<PagedResult<ResponseModel.CheckinDetailDto>> GetCheckinByUserAsync(Guid currentUserId, IList<string> currentUserRoles, Guid? staffId, int? day, int? month, int? year, int? pageIndex, int? pageSize);
        Task<PagedResult<UserWithCheckinsDto>> GetUsersWithCheckinsAsync(string? Name, Guid? companyId, Guid? departmentId, Guid? positionId, int? Day, int? Month, int? Year, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles);

    }
}
