using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.PositionServices;
namespace EmployeeAPI.Services.PositionServices
{
     public interface IPositionService
     {
         
        Task<PagedResult<ResponseModel.PositionDTO>> GetAllAsync(string? name, Guid? departmentId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRole);
        Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRole);
        Task<ResponseModel.PositionDTO> AddAsync(ResponseModel.CreatePositionDto dto, Guid currentUserId, IList<string> currentUserRole);
        Task<ResponseModel.PositionDTO> UpdateAsync(Guid id, string Name, Guid currentUserId, IList<string> currentUserRole);
        Task<string> SoftDeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRole);
        //Task<ResponseModel.PositionDTO> GetAllEmployee(string name);
        Task<PagedResult<ResponseModel.UserFilterDto>> GetStaffByPositionAsync(Guid positionId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRole);
    }
}
