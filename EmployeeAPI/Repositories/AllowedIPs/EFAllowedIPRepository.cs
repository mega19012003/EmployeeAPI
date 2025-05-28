using System.Net;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.AllowedIPs
{
    public class EFAllowedIPRepository : IAllowedIPRepository
    {
        private readonly AppDbContext _context;
        public EFAllowedIPRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<AllowedIP>> GetAllAsync()
        {
            return await _context.AllowedIPs.ToListAsync();
        }

        public async Task<AllowedIP> GetByIdAsync(Guid id)
        {
            return await _context.AllowedIPs.FindAsync(id);
        }

        public async Task AddAsync(AllowedIP entity)
        {
            await _context.AllowedIPs.AddAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.AllowedIPs.FindAsync(id);
            if (entity != null)
            {
                _context.AllowedIPs.Remove(entity);
            }
        }

        public async Task<bool> ExistsAsync(string ip)
        {
            return await _context.AllowedIPs.AnyAsync(a => a.IPAddress == ip);
        }
    }
}
