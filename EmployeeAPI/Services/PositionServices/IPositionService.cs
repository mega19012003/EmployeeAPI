using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.PositionServices;
using static EmployeeAPI.Services.PositionServices.ResponseModel;
namespace EmployeeAPI.Services.PositionServices
{
     public interface IPositionService
     {
        Task<PagedResult<ResponseModel.PositionDTO>> GetAllAsync(string? name, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRole);
        Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id);
        Task<ResponseModel.PositionDTO> AddAsync(ResponseModel.CreatePosition dto, Guid currentUserId, IList<string> currentUserRole);
        Task<ResponseModel.UpdatePosition> UpdateAsync(Guid id, string Name, Guid currentUserId, IList<string> currentUserRole);
        Task<string> SoftDeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRole);
        //Task<ResponseModel.PositionDTO> GetAllEmployee(string name);
        Task<PagedResult<UserFilter>> GetStaffByPositionAsync(Guid positionId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRole);
    }
}
