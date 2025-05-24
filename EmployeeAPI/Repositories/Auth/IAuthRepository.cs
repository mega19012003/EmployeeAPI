using EmployeeAPI.Models;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Repositories.Auth
{
    public interface IAuthRepository
    {
       Task<User> GetUserByName(string username);
        Task<User> LoginAsync(string username, string password);
        
        Task<User> GetLoginUserAsync(string username);
        IQueryable<User> GetAll();

        //Task<User> UpdateAsync(User user);
        //Task<User> RegisterAsync(User user, string password);
        //Task<User> GetByIdAsync(Guid id);
        //Task<User> GetByIdAsync(Guid id);
        //Task<User> SoftDeleteAsync(User user);

    }
}
