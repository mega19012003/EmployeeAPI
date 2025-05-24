using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User> UpdateAsync(User user);
        Task<User> GetByIdAsync(Guid id);
        IQueryable<User> GetAll();
        //Task<User> GetByIdAsync(Guid id);
        Task<User> SoftDeleteAsync(User user);
        Task<IEnumerable<User>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
    }
}
