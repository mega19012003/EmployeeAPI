using System.Transactions;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.PositionServices.ResponseModel;

namespace EmployeeAPI.Services.AllowedIpServices
{
    public class AllowedIPService : IAllowedIPService
    {
        private readonly IAllowedIPRepository _allowedIPRepository;
        private readonly AppDbContext _context;
        public AllowedIPService(IAllowedIPRepository allowedIPRepository, AppDbContext context)
        {
            _allowedIPRepository = allowedIPRepository;
            _context = context;
        }

        public async Task<bool> IsIpAllowedAsync(string ipAddress)
        {
            return await _allowedIPRepository.IsIpAllowedAsync(ipAddress);
        }

        public async Task<AllowedIP> AddAllowedIPAsync(string ipAddress)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                    throw new ArgumentException("IP address cannot be null or empty");

                var existingIPs = (await _allowedIPRepository.GetAllAllowedIPsAsync());
                if (existingIPs.Any(ip => ip.IPAddress == ipAddress))
                    throw new InvalidOperationException($"IP address {ipAddress} is already allowed.");

                var allowedIP = new AllowedIP
                {
                    AllowedIPId = Guid.NewGuid(),
                    IPAddress = ipAddress
                };

                await _allowedIPRepository.AddAllowedIPAsync(ipAddress);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return allowedIP;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<AllowedIP>> GetAllAllowedIPsAsync(string? ip, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.AllowedIPs.AsQueryable();

            if (!string.IsNullOrEmpty(ip))
            {
                query = query.Where(f => f.IPAddress.ToLower().Contains(ip.ToLower()));
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex.Value - 1) * pageSize.Value)
                                   .Take(pageSize.Value)
                                   .ToListAsync();

            return new PagedResult<AllowedIP>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }

    }
}
