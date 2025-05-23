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
        //Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetByIdAsync(Guid id);
        //Task<User> GetUserAsync(string username, string password, string fullname);
        Task<User> GetLoginUserAsync(string username);
        Task<IEnumerable<User>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm);
        //Task<User> GetByIdAsync(Guid id);
        //Task<User> AddAsync(User dto);
        
        //Task<Staff> SoftDeleteAsync(Guid staff);
        Task<User> SoftDeleteAsync(User user);
        //Task<IEnumerable<User>> GetByNameAsync(string name, int? pageSize, int? pageIndex);
    }
}
