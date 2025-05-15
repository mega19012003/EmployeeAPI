using EmployeeAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EmployeeAPI.Repositories.Auth
{
    public class EFAuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public EFAuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            user.Id = Guid.NewGuid();
            user.Password = HashPassword(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.Password != HashPassword(password))
                return null;
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserAsync(string username, string password, string fullname)
        {
            return await _context.Users.FirstOrDefaultAsync(p => p.Username == username && p.Fullname == fullname && p.Password == password);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

