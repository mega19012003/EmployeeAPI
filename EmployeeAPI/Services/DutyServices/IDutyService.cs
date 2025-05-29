using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;

namespace EmployeeAPI.Services.DutyServices
{
    public interface IDutyService
    {
        Task<PagedResult<ResponseModel.DutyDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, /*Guid? id,*/ string? name, int? pageSize, int? pageIndex);
        Task<ResponseModel.DutyDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDto> AddDutyAsync(ResponseModel.CreateDuty dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDto> AddDutyDetailAsync(ResponseModel.CreateDuty dto, Guid DutyId, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDto> UpdateDutyAsync(ResponseModel.UpdateDuty dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDetailDto> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetail dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteDutyAsync(Guid id);
        Task<string> SoftDeleteDutyDetailAsync(Guid Id);
        //WTask<ResponseModel.DutyDto> GetDutyByName(string name);
        //Task<ResponseModel.DutyDto> GetUnfinishedDuty(string status);
    }
}
