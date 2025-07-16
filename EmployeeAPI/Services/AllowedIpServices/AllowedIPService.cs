using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Net;
using System.Transactions;
using static EmployeeAPI.Repositories.AllowedIPs.ResponseModel;
using static EmployeeAPI.Services.PositionServices.ResponseModel;

namespace EmployeeAPI.Services.AllowedIpServices
{
    public class AllowedIPService : IAllowedIPService
    {
        private readonly IAllowedIPRepository _allowedIPRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        public AllowedIPService(IAllowedIPRepository allowedIPRepository, AppDbContext context, IUserRepository userRepository)
        {
            _allowedIPRepository = allowedIPRepository;
            _context = context;
            _userRepository = userRepository;
        }

        public async Task<PagedResult<IPDto>> GetAllAsync(string? IpAdress, Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            //return await _allowedIPRepository.GetAllAsync();
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _allowedIPRepository.GetAll();

            if (!string.IsNullOrEmpty(IpAdress))
            {
                var keyword = IpAdress.Trim().ToLower();
                query = query.Where(x => x.IPAddress.ToLower().Contains(keyword));
            }

            if (currentUserRoles.Contains("SystemAdmin"))
            {
                if (companyId.HasValue)
                {
                    query = query.Where(x => x.CompanyId == companyId.Value);
                }
            }
            else
            {
                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser.CompanyId == null)
                    throw new ArgumentException("Người dùng chưa có công ty.");
                //else if (!currentUserRoles.Contains("SystemAdmin") && currentUser.Department == null)
                //    throw new ArgumentException("Người dùng chưa có phòng ban.");

                var userCompanyId = currentUser.CompanyId.Value;
                query = query.Where(x => x.CompanyId == userCompanyId);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(f => new IPDto
                {
                    AllowedIPId = f.AllowedIPId,
                    IPAddress = f.IPAddress,
                    companyName = f.Company.Name,
                }).ToListAsync();
            return new PagedResult<IPDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }

        public async Task<IPDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var result = await _allowedIPRepository.GetByIdAsync(id);
            if (result == null)
                throw new ArgumentException("Không tìm thấy IP");

            if (!currentUserRoles.Contains("SystemAdmin"))
            {
                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser?.CompanyId == null)
                    throw new ArgumentException("Người dùng chưa có công ty.");
                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Người dùng chưa có phòng ban.");

                if (result.CompanyId != currentUser.CompanyId)
                    throw new ArgumentException("Không có quyền truy cập IP của công ty khác.");
            }

            return new IPDto
            {
                AllowedIPId = result.AllowedIPId,
                IPAddress = result.IPAddress,
                companyName = result.Company.Name,
            };
        }

        public async Task<IPDto> AddAsync(string ip, Guid currentUserId, IList<string> currentUserRoles)
        {
            var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
            if (currentUser?.CompanyId == null)
                throw new ArgumentException("Người dùng chưa có công ty.");

            var companyId = currentUser.CompanyId.Value;

            if (await _allowedIPRepository.ExistsAsync(ip, companyId))
                throw new ArgumentException("IP này đã tồn tại trong công ty.");

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
                CompanyId = companyId,
            };

            await _allowedIPRepository.AddAsync(allowedIP);
            await _context.SaveChangesAsync();

            return new IPDto
            {
                AllowedIPId = allowedIP.AllowedIPId,
                IPAddress = allowedIP.IPAddress,
                companyName = allowedIP.Company.Name
            };
        }

        public async Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var result = await _allowedIPRepository.GetByIdAsync(id);
            
            var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
            if (currentUser?.CompanyId == null)
                throw new ArgumentException("Người dùng chưa có công ty.");
            if (currentUser?.CompanyId != result.CompanyId)
                throw new ArgumentException("Chỉ được phép xóa cấu hình IP của công ty");

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

