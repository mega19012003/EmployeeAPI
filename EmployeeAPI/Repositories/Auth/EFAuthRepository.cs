using EmployeeAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Azure;
using EmployeeAPI.Services.AuthServices;

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
            if (user == null || user.Password != HashPassword(password))
                return null;

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
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
        /*public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }*/
        public async Task<User> GetLoginUserAsync(string username)
        {
            return await _context.Users.Include(p => p.Department).Include(p => p.Position).FirstOrDefaultAsync(p => p.Username == username);
        }

        //public async Task<User> UpdateAsync(User user)
        //{
        //    var existingUser = await _context.Users
        //        .Include(p => p.Department)
        //        .Include(p => p.Position)
        //        .FirstOrDefaultAsync(p => p.UserId == user.UserId && !p.IsDeleted && p.IsActive);

        //    if (existingUser == null)
        //        return null;

        //    existingUser.Fullname = user.Fullname;
        //    existingUser.Address = user.Address;
        //    existingUser.PhoneNumber = user.PhoneNumber;
        //    existingUser.DateOfBirth = user.DateOfBirth;
        //    existingUser.Role = user.Role;
        //    existingUser.BasicSalary = user.BasicSalary;
        //    existingUser.PositionId = user.PositionId;
        //    existingUser.DepartmentId = user.DepartmentId;
        //    existingUser.ImageUrl = user.ImageUrl;
        //    existingUser.IsActive = user.IsActive;

        //    // Bỏ SaveChangesAsync ở đây
        //    return existingUser;
        //}

        //public async Task<User> SoftDeleteAsync(User User)
        //{
        //    if (User == null)
        //        return null;

        //    User.IsDeleted = true;
        //    //User.IsActive = false;

        //    _context.Users.Update(User);

        //    return User;
        //}

        //{

        //    return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        //}
        //public async Task<IEnumerable<User>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex)
        //{
        //    var user = await _context.Users.Where(p => p.IsActive == true && p.IsDeleted != true).AsNoTracking().ToListAsync();

        //    return user;
        //}
    }
}

