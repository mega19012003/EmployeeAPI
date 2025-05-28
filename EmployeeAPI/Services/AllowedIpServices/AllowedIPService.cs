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

        public async Task<List<AllowedIP>> GetAllAsync()
        {
            return await _allowedIPRepository.GetAllAsync();
        }

        public async Task<AllowedIP> GetByIdAsync(Guid id)
        {
            return await _allowedIPRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(string ip)
        {
            if (await _allowedIPRepository.ExistsAsync(ip))
                throw new ArgumentException("IP đã tồn tại trong danh sách.");

            var allowedIP = new AllowedIP
            {
                AllowedIPId = Guid.NewGuid(),
                IPAddress = ip,
            };

            await _allowedIPRepository.AddAsync(allowedIP);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _allowedIPRepository.DeleteAsync(id);
            await _context.SaveChangesAsync();
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

