using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EmployeeAPI.Repositories.Users
{
    public class EFUserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public EFUserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid?> GetDepartmentIdByUserIdAsync(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.DepartmentId;
        }
        public async Task<User> UpdateAsync(User user)
        {
            var existingUser = await _context.Users
                .Include(p => p.Department)
                .Include(p => p.Position)
                .FirstOrDefaultAsync(p => p.UserId == user.UserId && !p.IsDeleted && p.IsActive);

            if (existingUser == null)
                return null;

            existingUser.Fullname = user.Fullname;
            existingUser.Address = user.Address;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Role = user.Role;
            existingUser.BasicSalary = user.BasicSalary;
            existingUser.PositionId = user.PositionId;
            existingUser.DepartmentId = user.DepartmentId;
            existingUser.ImageUrl = user.ImageUrl;
            existingUser.IsActive = user.IsActive;

            // Bỏ SaveChangesAsync ở đây
            return existingUser;
        }
        public async Task<User> SoftDeleteAsync(User User)
        {
            if (User == null)
                return null;

            User.IsDeleted = true;
            //User.IsActive = false;

            _context.Users.Update(User);

            return User;
        }

        public IQueryable<User> GetAll()
        {
            return _context.Users
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Where(u => u.IsActive && !u.IsDeleted)
                .AsNoTracking();
        }
        //public async Task<User> GetAllUser()
        //{
        //    return await _context.Users
        //        .Include(u => u.Department)
        //        .Include(u => u.Position)
        //        .FirstOrDefaultAsync(u => u.IsActive && !u.IsDeleted);
        //}
        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.Include(p => p.Department).Include(p => p.Position)
                  .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted && p.IsActive);
        }
        public async Task<IEnumerable<User>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex)
        {
            var user = await _context.Users.Where(p => p.IsActive && !p.IsDeleted).AsNoTracking().ToListAsync();

            return user;
        }
        //public async Task<User> GetByIdAsync(Guid id)
        //{
        //    return await _context.Users.Include(p => p.Department).Include(p => p.Position)
        //          .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted && p.IsActive);
        //}
        //public async Task<IEnumerable<User>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex)
        //{
        //    var user = await _context.Users.Where(p => p.IsActive == true && p.IsDeleted != true).AsNoTracking().ToListAsync();

        //    return user;
        //}
    }
}
