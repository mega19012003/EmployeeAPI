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
        public async Task<bool> IsIpAllowedAsync(string ipAddress)
        {
            return await _context.AllowedIPs.AnyAsync(ip => ip.IPAddress == ipAddress);
        }
        public async Task AddAllowedIPAsync(string ipAddress)
        {
            var allowedIP = new AllowedIP { IPAddress = ipAddress };
            _context.AllowedIPs.Add(allowedIP);
            //await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<AllowedIP>> GetAllAllowedIPsAsync()
        {
            return await _context.AllowedIPs.Where(p => !p.isDeleted).ToListAsync();
        }
    }
}
