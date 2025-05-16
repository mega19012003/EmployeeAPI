using EmployeeAPI.Models;
using EmployeeAPI.Services.PositionServices;
using static EmployeeAPI.Services.StaffServices.ResponseModel;
namespace EmployeeAPI.Services.PositionServices
{
     public interface IPositionService
     {
        Task<IEnumerable<ResponseModel.PositionDTO>> GetAllAsync();
        Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id);
        Task<ResponseModel.CreateAndUpdatePosition> AddAsync(string Name);
        Task<ResponseModel.CreateAndUpdatePosition> UpdateAsync(Guid id, string Name);
        Task<string> SoftDeleteAsync(Guid id);
        Task<ResponseModel.PositionDTO> GetAllEmployee(string name);
        Task<IEnumerable<StaffFilter>> GetStaffByPositionAsync(string positionName, int? pageSize, int? pageIndex);
    }
}
