using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Departments
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync(string? name, int? pageIndex, int? pageSize);
        Task<Department> GetByIdAsync(Guid id);
        Task AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task SoftDeleteAsync(Guid id);
        Task<IEnumerable<Department>> GetDepartmentByName(string name);
        Task<IEnumerable<Department>> GetStaffByDepartmentAsync(string positionName, int? pageSize, int? pageIndex);
        Task<IEnumerable<Department>> GetPositionsByDepartmentAsync(Guid? id, int? pageSize, int? pageIndex);
    }
}
