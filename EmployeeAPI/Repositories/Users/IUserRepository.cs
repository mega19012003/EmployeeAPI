using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Users
{
    public interface IUserRepository
    {
        //Task<Guid?> GetDepartmentIdByUserIdAsync(Guid userId);
        Task<User> UpdateAsync(User user);
        Task<User> GetUserInfoAsync(Guid id);
        Task<User> GetActiveUserIdAsync(Guid id);
        IQueryable<User> GetAll();
        //Task<User> GetAllUser();
        //Task<User> GetByIdAsync(Guid id);
        //Task<User> SoftDeleteAsync(User user);
        Task<IEnumerable<User>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
    }
}
