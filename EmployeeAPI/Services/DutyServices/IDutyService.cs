using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;

namespace EmployeeAPI.Services.DutyServices
{
    public interface IDutyService
    {
        Task<PagedResult<ResponseModel.DutyDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize);
        Task<ResponseModel.DutyDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDto> AddDutyAsync(ResponseModel.CreateDuty dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDto> AddDutyDetailAsync(ResponseModel.GetDutyDto dto, Guid DutyId, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDto> UpdateDutyAsync(ResponseModel.UpdateDuty dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDetailDto> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetail dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteDutyAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteDutyDetailAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles);
        //WTask<ResponseModel.DutyDto> GetDutyByName(string name);
        //Task<ResponseModel.DutyDto> GetUnfinishedDuty(string status);
    }
}
