using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Staffs
{
    public interface IStaffRepository
    {
        Task<IEnumerable<Staff>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm);
        Task<Staff> GetByIdAsync(Guid id);
        Task<Staff> AddAsync(Staff dto);
        Task<Staff> UpdateAsync(Staff staff);
        Task<Staff> SoftDeleteAsync(Guid staff);
        Task<IEnumerable<Staff>> GetByNameAsync(string name, int? pageSize, int? pageIndex);
        Task<IEnumerable<Staff>> GetEmployeeByPosition(string SearchTerm, int? pageSize, int? pageIndex);
    }
}
