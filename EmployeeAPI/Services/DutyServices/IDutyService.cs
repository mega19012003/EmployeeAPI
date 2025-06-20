using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;

namespace EmployeeAPI.Services.DutyServices
{
    public interface IDutyService
    {
        Task<PagedResult<ResponseModel.DutyResultDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize);
        Task<ResponseModel.DutyResultDto> GetDutyByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDetailResultDto> GetDutyDetailByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyResultDto> AddDutyAsync(ResponseModel.CreateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyResultDto> AddDutyDetailAsync(ResponseModel.GetDutyDto dto, Guid DutyId, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyResultDto> UpdateDutyAsync(ResponseModel.UpdateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDetailResultDto> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetailDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteDutyAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteDutyDetailAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles);
        //WTask<ResponseModel.DutyResultDto> GetDutyByName(string name);
        //Task<ResponseModel.DutyResultDto> GetUnfinishedDuty(string status);
    }
}
