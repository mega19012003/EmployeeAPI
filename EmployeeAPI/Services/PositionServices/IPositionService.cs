using EmployeeAPI.Models;

namespace EmployeeAPI.Services.PositionServices
{
    public interface IPositionService
    {
        public Task<IEnumerable<ResponseModel.PositionDTO>> GetAllAsync();
        public Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id);
        public Task<ResponseModel.CreatePosition> AddAsync(string Name);
        public Task<ResponseModel.UpdatePosition> UpdateAsync(Guid id, string Name);
        public Task<string> SoftDeleteAsync(Guid id);
        public Task<ResponseModel.PositionDTO> GetAllEmployee(string name);
    }
}
