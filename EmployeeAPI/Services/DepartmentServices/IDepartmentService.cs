using EmployeeAPI.Models;
using static EmployeeAPI.Services.StaffServices.ResponseModel;

namespace EmployeeAPI.Services.DepartmentServices
{
    public interface IDepartmentService
    {
        public Task<IEnumerable<ResponseModel.DepartmentDto>> GetAllAsync();
        public Task<ResponseModel.DepartmentDto> GetByIdAsync(Guid id);
        public Task<ResponseModel.CreateDepartment> AddAsync(string Name);
        public Task<ResponseModel.UpdateDepartment> UpdateAsync(Guid id, string Name);
        public Task<string> SoftDeleteAsync(Guid id);
        public Task<IEnumerable<ResponseModel.DepartmentDto>> GetDepartmentByName(string name);
        public Task<IEnumerable<StaffFilter>> GetStaffByDepartmentAsync(string positionName, int? pageSize, int? pageIndex);
    }
}