using EmployeeAPI.Models;

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
        Task<Department?> GetAllEmployee(string name);
    }
}
