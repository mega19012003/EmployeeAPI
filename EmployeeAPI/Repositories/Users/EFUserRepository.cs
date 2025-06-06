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
            _context.Users.Update(user);

            return user;
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
            var result = _context.Users
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Where(u => u.IsActive && !u.IsDeleted)
                .AsNoTracking();


            return result;
        }
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
