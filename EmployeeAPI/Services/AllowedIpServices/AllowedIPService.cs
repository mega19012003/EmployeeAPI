using System.Net;
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

        public async Task<PagedResult<AllowedIP>> GetAllAsync(string? IpAdress, int? pageIndex, int? pageSize)
        {
            //return await _allowedIPRepository.GetAllAsync();
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.AllowedIPs
                .Where(f => string.IsNullOrEmpty(IpAdress) || f.IPAddress.ToLower().Contains(IpAdress.ToLower()));

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(f => new AllowedIP
                {
                    AllowedIPId = f.AllowedIPId,
                    IPAddress = f.IPAddress,
                }).ToListAsync();
            return new PagedResult<AllowedIP>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }

        public async Task<AllowedIP> GetByIdAsync(Guid id)
        {
            var result = await _allowedIPRepository.GetByIdAsync(id);
            if (result == null)
                throw new ArgumentException("Không tìm thấy IP");

            return new AllowedIP
            {
                AllowedIPId = result.AllowedIPId,
                IPAddress = result.IPAddress
            };
        }

        public async Task<AllowedIP> AddAsync(string ip)
        {
            if (await _allowedIPRepository.ExistsAsync(ip))
                throw new ArgumentException("IP này đã tồn tại");

            // Kiểm tra IP cụ thể
            bool isSpecificIP = IPAddress.TryParse(ip, out var _);

            // Kiểm tra dải IP (CIDR), ví dụ: "192.168.1.0/24"
            bool isCIDR = false;
            if (ip.Contains("/"))
            {
                var parts = ip.Split('/');
                if (parts.Length == 2 &&
                    IPAddress.TryParse(parts[0], out _) &&
                    int.TryParse(parts[1], out int prefixLength) &&
                    prefixLength >= 0 && prefixLength <= 32)
                {
                    isCIDR = true;
                }
            }

            if (!isSpecificIP && !isCIDR)
                throw new ArgumentException("Định dạng IP không hợp lệ. Vui lòng nhập IP cụ thể (ví dụ: 192.168.1.1) hoặc dải IP CIDR (ví dụ: 192.168.1.0/24).");

            var allowedIP = new AllowedIP
            {
                AllowedIPId = Guid.NewGuid(),
                IPAddress = ip,
            };

            await _allowedIPRepository.AddAsync(allowedIP);
            await _context.SaveChangesAsync();

            return new AllowedIP
            {
                AllowedIPId = allowedIP.AllowedIPId,
                IPAddress = allowedIP.IPAddress
            };
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var result = await _allowedIPRepository.GetByIdAsync(id);
            await _allowedIPRepository.DeleteAsync(id);
            await _context.SaveChangesAsync();

            return "Đã xóa IP " + result.IPAddress;
        }

        public async Task<bool> IsIPAllowedAsync(string ip)
        {
            var allowedIps = await _allowedIPRepository.GetAllAsync();

            foreach (var allowed in allowedIps)
            {
                if (allowed.IPAddress.Contains("/"))
                {
                    if (IsIpInCidr(ip, allowed.IPAddress))
                        return true;
                }
                else
                {
                    if (ip == allowed.IPAddress)
                        return true;
                }
            }

            return false;
        }

        public bool IsIpInCidr(string ipAddress, string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2)
                return false;

            var networkAddress = parts[0];
            var prefixLength = int.Parse(parts[1]);

            var ip = IPAddress.Parse(ipAddress);
            var network = IPAddress.Parse(networkAddress);

            var ipBytes = ip.GetAddressBytes();
            var networkBytes = network.GetAddressBytes();

            if (ipBytes.Length != networkBytes.Length)
                return false;

            int byteCount = prefixLength / 8;
            int bitRemainder = prefixLength % 8;

            for (int i = 0; i < byteCount; i++)
            {
                if (ipBytes[i] != networkBytes[i])
                    return false;
            }

            if (bitRemainder > 0)
            {
                int mask = (byte)~(255 >> bitRemainder);
                if ((ipBytes[byteCount] & mask) != (networkBytes[byteCount] & mask))
                    return false;
            }

            return true;
        }
    }
}

