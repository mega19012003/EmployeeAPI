using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Holidays;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.HolidayServices;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace EmployeeAPI.Services.HolidayServices
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly ILogger<HolidayService> _logger;
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        public HolidayService(IHolidayRepository holidayRepository, ILogger<HolidayService> logger , AppDbContext context, IUserRepository userRepository)
        {
            _holidayRepository = holidayRepository;
            _logger = logger;
            _context = context;
            _userRepository = userRepository;
        }
        public async Task<PagedResult<ResponseModel.HolidayResultDto>> GetAllAsync(string? name, Guid? companyId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _holidayRepository.GetAll();

                if(!currentUserRoles.Contains("SystemAdmin"))
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser.CompanyId == null)
                        throw new ArgumentException("Chưa có công ty, vui lòng liên hệ admin để thêm");

                    companyId = currentUser.CompanyId;

                    query = query.Where(p => p.CompanyId == companyId);
                }
                else if(companyId.HasValue)
                    query = query.Where(p => p.CompanyId == companyId);

                if (!string.IsNullOrEmpty(name))
                {
                    string loweredName = name.ToLower();
                    query = query.Where(h => h.name.ToLower().Contains(loweredName));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.HolidayResultDto
                    {
                        HolidayId = f.Id,
                        Name = f.name,
                        startDate = f.startDate,
                        endDate = f.endDate,
                        companyName = f.Company.Name
                    })
                    .ToListAsync();

                return new PagedResult<ResponseModel.HolidayResultDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving holiday. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<ResponseModel.HolidayResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var holiday = await _holidayRepository.GetByIdAsync(id);
                if (holiday == null)
                {
                    throw new ArgumentException("Không tìm thấy ngày lễ");
                }

                if (currentUserRoles.Contains("Administrator"))
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser.CompanyId == null)
                        throw new ArgumentException("Chưa có công ty, vui lòng liên hệ Admin hoặc System Admin  để thêm");
                    if (holiday.CompanyId != currentUser.CompanyId)
                        throw new ArgumentException("Chỉ được phép truy cập thông tin ngày nghỉ cùng công ty");
                }

                return new ResponseModel.HolidayResultDto
                {
                    HolidayId = holiday.Id,
                    Name = holiday.name,
                    startDate = holiday.startDate,
                    endDate = holiday.endDate,
                    companyName = holiday.Company.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving holiday by id. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        
        public async Task<ResponseModel.HolidayResultDto> CreateAsync(ResponseModel.CreateHolidayDto dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.Name == null || dto.endDate == null || dto.startDate == null)
                {
                    throw new ArgumentException("Holiday input invalid");
                }

                if(dto.startDate > dto.endDate)
                {
                    throw new ArgumentException("Ngày bắt đầu không được để sau ngày kết thúc");
                }

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser.CompanyId == null)
                    throw new ArgumentException("Chưa có công ty, vui lòng liên hệ Sytem Admin để thêm");

                var model = new Models.Holiday
                {
                    Id = Guid.NewGuid(),
                    name = dto.Name,
                    startDate = dto.startDate,
                    endDate = dto.endDate,
                    CompanyId = (Guid)currentUser.CompanyId,
                };

                await _holidayRepository.CreateAsync(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.HolidayResultDto
                {
                    HolidayId = model.Id,
                    Name = model.name,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    companyName = model.Company.Name
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding holiday. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.HolidayResultDto> UpdateAsync(ResponseModel.UpdateHolidayDto dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _holidayRepository.GetByIdAsync(dto.HolidayId);
                if (result == null)
                    throw new ArgumentException("Không thể tìm thấy ngày lễ");

                if(dto.startDate > dto.endDate)
                {
                    throw new ArgumentException("Ngày bắt đầu không được để sau ngày kết thúc");
                }

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser.CompanyId == null)
                    throw new ArgumentException("Chưa có công ty, vui lòng liên hệ System Admin để thêm");
                if (currentUser.CompanyId != result.CompanyId)
                    throw new ArgumentException("Chỉ được phép cập nhật ngày lễ của công ty");

                result.name = dto.Name;
                result.startDate = dto.startDate;
                result.endDate = dto.endDate;

                await _holidayRepository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.HolidayResultDto
                {
                    HolidayId = result.Id,
                    Name = result.name,
                    startDate = result.startDate,
                    endDate = result.endDate,
                    companyName = result.Company.Name
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating holiday. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        
        public async Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _holidayRepository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Không thể tìm thấy ngày lễ");
                
                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser.CompanyId == null)
                    throw new ArgumentException("Chưa có công ty, vui lòng liên hệ System Admin để thêm");
                if (currentUser.CompanyId != result.CompanyId)
                    throw new ArgumentException("Chỉ được phép xóa ngày lễ của công ty");


                result.IsDeleted = true;
                await _holidayRepository.SoftDeleteAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "Đã xóa ngày lễ" + result.name;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting holiday. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
