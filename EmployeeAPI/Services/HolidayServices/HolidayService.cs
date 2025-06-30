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
        public async Task<PagedResult<ResponseModel.HolidayResultDto>> GetAllAsync(string? name, int? pageSize, int? pageIndex)
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
                    .Select(f => new ResponseModel.HolidayResultDto
                    {
                        HolidayId = f.Id,
                        Name = f.name,
                        startDate = f.startDate,
                        endDate = f.endDate
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
        public async Task<ResponseModel.HolidayResultDto> GetByIdAsync(Guid id)
        {
            try
            {
                var holiday = await _holidayRepository.GetByIdAsync(id);

                return new ResponseModel.HolidayResultDto
                {
                    HolidayId = holiday.Id,
                    Name = holiday.name,
                    startDate = holiday.startDate,
                    endDate = holiday.endDate,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving holiday by id. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        
        public async Task<ResponseModel.HolidayResultDto> CreateAsync(ResponseModel.CreateHolidayDto dto)
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
                    throw new ArgumentException("Start date cannot be after end date");
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

                return new ResponseModel.HolidayResultDto
                {
                    HolidayId = model.Id,
                    Name = model.name,
                    startDate = model.startDate,
                    endDate = model.endDate,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding holiday. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.HolidayResultDto> UpdateAsync(ResponseModel.UpdateHolidayDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _holidayRepository.GetByIdAsync(dto.HolidayId);
                if (result == null)
                    throw new ArgumentException("Cannot find holiday");

                if(dto.startDate > dto.endDate)
                {
                    throw new ArgumentException("Start date cannot be after end date");
                }

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
