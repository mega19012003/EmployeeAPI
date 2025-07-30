using EmployeeAPI.Models;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Repositories.Auth
{
    public interface IAuthRepository
    {
        Task<User> GetUserByName(string username);
        Task<User> LoginAsync(string username);
        Task UpdateUserAsync(User user);
        Task<User> GetLoginUserAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        //IQueryable<User> GetAll();
        Task<User> GetByIdAsync(Guid id);

    }
}
