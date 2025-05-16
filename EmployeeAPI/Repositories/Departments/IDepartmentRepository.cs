using EmployeeAPI.Models;
using static EmployeeAPI.Services.StaffServices.ResponseModel;

namespace EmployeeAPI.Repositories.Departments
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department> AddAsync(Department department);
        Task<Department?> UpdateAsync(Department department);
        Task<Department?> SoftDeleteAsync(Guid id);
        Task<IEnumerable<Department>> GetDepartmentByName(string name);
        Task<IEnumerable<Department>> GetStaffByDepartmentAsync(string positionName, int? pageSize, int? pageIndex);
    }
}
