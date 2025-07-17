using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.LogStatusConfigs;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.LogStatusConfigServices;
using EmployeeAPI.Services.UserService;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.LogStatusConfigServices.ResponseModel;

namespace EmployeeAPI.Services.LogStatusConfigServices
{
    public class LogStatusConfigService : ILogStatusConfigService
    {
        private readonly ILogStatusConfigRepository _repository;
        private readonly AppDbContext _context;
        private readonly ILogger<ILogStatusConfigService> _logger;
        private readonly IUserRepository _userRepo;

        public LogStatusConfigService(ILogStatusConfigRepository repository, AppDbContext context, ILogger<ILogStatusConfigService> logger, IUserRepository userRepo)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
            _userRepo = userRepo;
        }

        public async Task<PagedResult<ResponseModel.LogStatusDto>> GetAllConfigsAsync(string? name, Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var isSystemAdmin = currentUserRoles.Contains("SystemAdmin");
               
                var query = _context.LogStatusConfigs
                    .Where(f => !f.IsSystemDefault && (string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower())));

                if (!isSystemAdmin)
                {
                    var currentUser = await _userRepo.GetActiveUserIdAsync(currentUserId);

                    if (currentUser == null || currentUser.CompanyId == null)
                        throw new ArgumentException("Người dùng hiện tại chưa thuộc công ty nào. Vui lòng liên hệ System admin để cập nhật công ty.");

                    query = query.Where(p => p.CompanyId == currentUser.CompanyId);
                }
                else
                {
                    if (companyId.HasValue)
                    {
                        query = query.Where(p => p.CompanyId == companyId.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Include(p => p.Company)
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.LogStatusDto
                    {
                        Id = f.Id,
                        enumId = f.enumId,
                        Name = f.Name,
                        SalaryMultiplier = f.SalaryMultiplier,
                        Note = f.Note,
                        //CompanyId = f.CompanyId,
                        CompanyName = f.Company.Name,
                       
                    }).ToListAsync();
                return new PagedResult<ResponseModel.LogStatusDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving log status. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.LogStatusDto> GetConfigIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var isAdmin = currentUserRoles.Contains("Administrator");

            var result = await _repository.GetByIdAsync(id);

            if (result == null)
                throw new ArgumentException("Cấu hình log status không tìm thấy");

            if (!isAdmin)
            {
                var currentUser = await _userRepo.GetActiveUserIdAsync(currentUserId);

                if (currentUser == null || currentUser.CompanyId == null)
                    throw new ArgumentException("Người dùng hiện tại chưa thuộc công ty nào. Vui lòng liên hệ admin để cập nhật công ty.");

                if (result.CompanyId != currentUser.CompanyId)
                    throw new ArgumentException("Bạn không có quyền xem cấu hình log status của công ty khác.");
            }

            return new ResponseModel.LogStatusDto
            {
                Id = result.Id,
                enumId = result.enumId,
                Name = result.Name,
                SalaryMultiplier = result.SalaryMultiplier,
                Note = result.Note,
                //CompanyId = result.CompanyId,
                CompanyName = result.Company?.Name
            };
        }

        public async Task<LogStatusDto> UpdateConfigAsync(UpdateLogStatusDto updatedConfig)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _repository.GetByIdAsync(updatedConfig.Id);
                if (existing == null)
                    throw new ArgumentException("Cấu hình log status không tìm thấy");

                existing.SalaryMultiplier = updatedConfig.SalaryMultiplier;
                existing.Name = updatedConfig.Name;
                existing.Note = updatedConfig.Note;

                await _repository.UpdateAsync(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new LogStatusDto
                {
                    Id = existing.Id,
                    enumId = existing.enumId,
                    Name = existing.Name,
                    SalaryMultiplier = existing.SalaryMultiplier,
                    Note = existing.Note,
                    //CompanyId = existing.CompanyId,
                    CompanyName = existing.Company.Name
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ArgumentException("Invalid input data for checkin status config update. Please check the provided values and try again.", ex);
            }
        }
    }
}
