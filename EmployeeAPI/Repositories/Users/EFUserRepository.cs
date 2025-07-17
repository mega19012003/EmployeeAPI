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
        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return user;
        }
        public IQueryable<User> GetAll()
        {
            var result = _context.Users
                .Include(u => u.Company)
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Where(u => !u.IsDeleted)
                .AsNoTracking();
            return result;
        }
        
        //lấy thông tin nhân viên, lấy dc cả thông tin nhân viên đã nghỉ việc 
        public async Task<User> GetUserInfoAsync(Guid id)
        {
            return await _context.Users
                .Include(p => p.Company)
                .Include(p => p.Department)
                .Include(p => p.Position)
                 .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);
        }

        //lấy thông tin nhân viên còn làm việc để dùng cho các chức năng khác
        public async Task<User> GetActiveUserIdAsync(Guid id)
        {
            return await _context.Users
                .Include(p => p.Company)
                .Include(p => p.Department)
                .Include(p => p.Position)
                .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted && p.IsActive);
        }

        public async Task<IEnumerable<User>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex)
        {
            var user = await _context.Users.Where(p => p.IsActive && !p.IsDeleted).AsNoTracking().ToListAsync();
            return user;
        }

    }
}
