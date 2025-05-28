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

        public async Task<PagedResult<ResponseModel.IPDto>> GetAllAllowedIPsAsync(string? ip, int? pageIndex, int? pageSize)
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
                                   .Select(f => new ResponseModel.IPDto
                                   {
                                       AllowedIPId = f.AllowedIPId,
                                       IPAddress = f.IPAddress
                                   })
                                   .ToListAsync();

            return new PagedResult<ResponseModel.IPDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }

        public async Task<ResponseModel.IPDto> AddAllowedIPAsync(string ipAddress)
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

                return new ResponseModel.IPDto {
                    AllowedIPId = allowedIP.AllowedIPId,
                    IPAddress = allowedIP.IPAddress
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }

        public async Task<ResponseModel.IPDto> UpdateAllowedIPAsync(ResponseModel.IPDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (dto.AllowedIPId == null)
                    throw new ArgumentException("Allowed IP cannot be null");

                /*var existingIPs = await _allowedIPRepository.GetAllAllowedIPsAsync();
                if (!existingIPs.Any(p => p.AllowedIPId == allowedIP.AllowedIPId))
                    throw new InvalidOperationException($"Allowed IP with ID {allowedIP.AllowedIPId} does not exist.");*/

                var query = await _allowedIPRepository.GetAllowedIPAsync(dto.AllowedIPId);
                if (query.AllowedIPId == null)
                    throw new Exception($"IP address {dto.IPAddress} does not existed");


                query.IPAddress = dto.IPAddress;

                //_context.AllowedIPs.Update(allowedIP);
                await _allowedIPRepository.UpdateAllowedIpAsync(query);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ResponseModel.IPDto
                {
                    AllowedIPId = query.AllowedIPId,
                    IPAddress = query.IPAddress
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }

        public async Task<string> DeleteAllowedIPAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var query = await _allowedIPRepository.GetAllowedIPAsync(id);
                if (query.AllowedIPId == null)
                    throw new Exception($"IP address {id} does not exist");

                query.isDeleted = true;

                await _allowedIPRepository.DeleteAllowedIPAsync(id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa ip: " + id + " thành công";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }

    }
}
