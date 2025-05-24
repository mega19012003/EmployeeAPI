using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.PositionServices;
using static EmployeeAPI.Services.AuthServices.ResponseModel;
using static EmployeeAPI.Services.UserService.ResponseModel;
namespace EmployeeAPI.Services.PositionServices
{
     public interface IPositionService
     {
        Task<PagedResult<ResponseModel.PositionDTO>> GetAllAsync(string? SearchTerm, int? pageIndex, int? pageSize);
        Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id);
        Task<ResponseModel.PositionDTO> AddAsync(ResponseModel.CreatePosition dto);
        Task<ResponseModel.UpdatePosition> UpdateAsync(Guid id, string Name);
        Task<string> SoftDeleteAsync(Guid id);
        //Task<ResponseModel.PositionDTO> GetAllEmployee(string name);
        Task<PagedResult<UserFilter>> GetStaffByPositionAsync(Guid departmentId, Guid positionId, int? pageSize, int? pageIndex);
    }
}
