using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.PositionServices;
using static EmployeeAPI.Services.PositionServices.ResponseModel;
namespace EmployeeAPI.Services.PositionServices
{
     public interface IPositionService
     {
        Task<PagedResult<ResponseModel.PositionDTO>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageIndex, int? pageSize);
        Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id);
        Task<ResponseModel.PositionDTO> AddAsync(ResponseModel.CreatePosition dto, ClaimsPrincipal claim);
        Task<ResponseModel.UpdatePosition> UpdateAsync(Guid id, string Name, ClaimsPrincipal claim);
        Task<string> SoftDeleteAsync(Guid id, ClaimsPrincipal claim);
        //Task<ResponseModel.PositionDTO> GetAllEmployee(string name);
        Task<PagedResult<UserFilter>> GetStaffByPositionAsync(Guid? departmentId, Guid positionId, int? pageSize, int? pageIndex);
    }
}
