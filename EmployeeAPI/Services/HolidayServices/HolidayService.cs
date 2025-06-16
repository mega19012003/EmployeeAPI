using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Holidays;
using EmployeeAPI.Services.HolidayServices;
using Microsoft.EntityFrameworkCore;



namespace EmployeeAPI.Services.HolidayServices
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly ILogger<HolidayService> _logger;
        private readonly AppDbContext _context;
        public HolidayService(IHolidayRepository holidayRepository, ILogger<HolidayService> logger , AppDbContext context)
        {
            _holidayRepository = holidayRepository;
            _logger = logger;
            _context = context;
        }
        public async Task<PagedResult<ResponseModel.HolidayDto>> GetAllAsync(string? name, int? pageSize, int? pageIndex)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _holidayRepository.GetAll();

                if (!string.IsNullOrEmpty(name))
                {
                    string loweredName = name.ToLower();
                    query = query.Where(h => h.name.ToLower().Contains(loweredName));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.HolidayDto
                    {
                        HolidayId = f.Id,
                        Name = f.name,
                        IsDeleted = f.IsDeleted,
                        startDate = f.startDate,
                        endDate = f.endDate
                    })
                    .ToListAsync();

                return new PagedResult<ResponseModel.HolidayDto>
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
        public async Task<ResponseModel.HolidayDto> GetByIdAsync(Guid id)
        {
            try
            {
                var holiday = await _holidayRepository.GetByIdAsync(id);

                return new ResponseModel.HolidayDto
                {
                    HolidayId = holiday.Id,
                    Name = holiday.name,
                    startDate = holiday.startDate,
                    endDate = holiday.endDate,
                    IsDeleted = holiday.IsDeleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving holiday by id. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<ResponseModel.HolidayDto> CreateAsync(ResponseModel.CreateHoliday dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.Name == null || dto.endDate == null || dto.startDate == null)
                {
                    throw new ArgumentException("Holiday input invalid");
                }

                var model = new Models.Holiday
                {
                    Id = Guid.NewGuid(),
                    name = dto.Name,
                    startDate = dto.startDate,
                    endDate = dto.endDate,

                };

                await _holidayRepository.CreateAsync(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.HolidayDto
                {
                    HolidayId = model.Id,
                    Name = model.name,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    IsDeleted = model.IsDeleted
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding holiday. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<ResponseModel.HolidayDto> UpdateAsync(ResponseModel.UpdateHoliday dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _holidayRepository.GetByIdAsync(dto.HolidayId);
                if (result == null)
                    throw new ArgumentException("Cannot find holiday");

                result.name = dto.Name;
                result.startDate = dto.startDate;
                result.endDate = dto.endDate;

                await _holidayRepository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.HolidayDto
                {
                    HolidayId = result.Id,
                    Name = result.name,
                    startDate = result.startDate,
                    endDate = result.endDate,
                    IsDeleted = result.IsDeleted
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating holiday. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<string> DeleteAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _holidayRepository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find holiday id");
                result.IsDeleted = true;
                await _holidayRepository.SoftDeleteAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "Delete holiday" + result.name + " success";
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
