using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static EmployeeAPI.Services.DutyServices.ResponseModel;

namespace EmployeeAPI.Services.DutyServices
{
    public interface IDutyService
    {
        Task<PagedResult<DutyResultDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, Guid? companyId, int? Day, int? Month, int? Year, string? filterStatus, int? pageIndex, int? pageSize);
        Task<ResponseModel.DutyResultDto> GetDutyByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDetailResultDto> GetDutyDetailByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyResultDto> AddDutyAsync(ResponseModel.CreateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyResultDto> AddDutyDetailAsync(ResponseModel.GetDutyDto dto, Guid DutyId, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyResultDto> UpdateDutyAsync(ResponseModel.UpdateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles);
        //Task<ResponseModel.DutyResultDto> MarkDutyAsCompletedAsync(Guid dutyId, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.DutyDetailResultDto> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetailDto dto, Guid currentUserId, IList<string> currentUserRoles);
        //Task<string> MarkDutyDetailAsCompletedAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteDutyAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteDutyDetailAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles);
    }
}
