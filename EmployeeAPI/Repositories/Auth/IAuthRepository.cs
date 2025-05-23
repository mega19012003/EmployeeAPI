using EmployeeAPI.Models;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Repositories.Auth
{
    public interface IAuthRepository
    {
        //Task<User> RegisterAsync(User user, string password);
        Task<User> GetUserByName(string username);
        Task<User> LoginAsync(string username, string password);
        Task<User> UpdateAsync(User user);
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetLoginUserAsync(string username);
        Task<IEnumerable<User>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
        //Task<User> GetByIdAsync(Guid id);
        Task<User> SoftDeleteAsync(User user);
        
    }
}
