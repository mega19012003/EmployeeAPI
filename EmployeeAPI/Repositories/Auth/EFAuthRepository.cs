using EmployeeAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Azure;
using EmployeeAPI.Services.AuthServices;
using EmployeeAPI.Helpers;

namespace EmployeeAPI.Repositories.Auth
{
    public class EFAuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public EFAuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            //if (user == null || !HashPassword.Verify(user.Password, password))
            //    return null;

            return user;
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public IQueryable<User> GetAll()
        {
            return _context.Users
                .AsNoTracking()
                .Include(p => p.Department)
                .Include(p => p.Position)
                .Where(p => !p.IsDeleted && p.IsActive);
        }
        public async Task<User> GetUserByName(string username)
        {
            /*var result = await _context.Users
                .Include(p => p.Department)
                .Include(p => p.Position)
                .FirstOrDefaultAsync(p => p.Username == users.Username);

            if (result != null)
                return null;

            return result;*/
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            var results = await _context.Users.Include(p => p.Department).Include(p => p.Position).FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted && p.IsActive);
            return results;
        }
        public async Task<User> GetLoginUserAsync(string username)
        {
            return await _context.Users.Include(p => p.Department).Include(p => p.Position).FirstOrDefaultAsync(p => p.Username == username);
        }
    }
}

