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
        public async Task<IEnumerable<AllowedIP>> GetAllAllowedIPsAsync()
        {
            return await _context.AllowedIPs/*.Where(p => !p.isDeleted)*/.ToListAsync();
        }
        public async Task<AllowedIP> GetAllowedIPAsync(Guid IPId)
        {
            return await _context.AllowedIPs.FirstOrDefaultAsync(ip => ip.AllowedIPId == IPId && !ip.isDeleted);
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
        public async Task UpdateAllowedIpAsync(AllowedIP allowedIP)
        {
            _context.AllowedIPs.Update(allowedIP);
            //await _context.SaveChangesAsync();
        }
        public async Task DeleteAllowedIPAsync(Guid IPId)
        {
            var allowedIP = await _context.AllowedIPs.FirstOrDefaultAsync(ip => ip.AllowedIPId == IPId);
            if (allowedIP != null)
            {
                _context.AllowedIPs.Update(allowedIP);
                //await _context.SaveChangesAsync();
            }
        }
    }
}
