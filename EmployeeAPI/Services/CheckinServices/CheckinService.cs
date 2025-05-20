using System.Runtime.CompilerServices;
using System.Transactions;
using Azure;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Staffs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;

namespace EmployeeAPI.Services.CheckinServices
{
    public class CheckinService : ICheckinService
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IStaffRepository _staffcheckinRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<CheckinService> _logger;

        public CheckinService(ICheckinRepository checkinRepository, IStaffRepository staffcheckinRepository, AppDbContext context, ILogger<CheckinService> logger)
        {
            _checkinRepository = checkinRepository;
            _staffcheckinRepository = staffcheckinRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ResponseModel.CheckinDto>> GetAllAsync(string? StaffName, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _context.Checkins
                    .Include(c => c.Staff)
                    .Where(f => string.IsNullOrEmpty(StaffName) || f.Staff.Name.ToLower().Contains(StaffName.ToLower()))
                    .Where(p => !p.IsDeleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.CheckinDto
                    {
                        CheckinId = c.Id,
                        CheckinDate = c.CheckinDate,
                        Status = c.Status,
                        StaffId = c.StaffId,
                        StaffName = c.Staff.Name,
                    }).ToListAsync();

                return new PagedResult<ResponseModel.CheckinDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving checkon. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.CheckinDto> GetByIdAsync(Guid id)
        {
            try
            {
                var c = await _checkinRepository.GetByIdAsync(id);
                if (c == null) return null;

                return new ResponseModel.CheckinDto
                {
                    CheckinDate = c.CheckinDate,
                    Status = c.Status,
                    StaffId = c.StaffId,
                    StaffName = c.Staff.Name,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving checkin by id. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.CheckinDto> CreateAsync(ResponseModel.CreateCheckin dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            { 
                //var checkins = await _checkinRepository.GetAllAsync();
                var exists = await _checkinRepository.ExistAsync(dto.StaffId);
                if (exists)
                    return null;
                /*var exists = await _checkinRepository.ExistsAsync(c =>
        c.StaffId == dto.StaffId && EF.Functions.DateDiffDay(c.CheckinDate, dto.CheckinDate) == 0);*/

                var existStaff = await _staffcheckinRepository.GetByIdAsync(dto.StaffId);
                if (existStaff == null)
                    return null;

                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    CheckinDate = dto.CheckinDate,
                    Status = dto.Status,
                    StaffId = dto.StaffId,
                };

                await _checkinRepository.CreateAsync(checkin);
                await _context.SaveChangesAsync(); //nhớ xóa savechang trong repository

                await transaction.CommitAsync();

                var staff = await _staffcheckinRepository.GetByIdAsync(dto.StaffId);
                return new ResponseModel.CheckinDto
                {
                    CheckinId = checkin.Id,
                    CheckinDate = checkin.CheckinDate,
                    Status = checkin.Status,
                    StaffId = checkin.StaffId,
                    StaffName = staff.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto)
        {
 
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(dto.CheckinId);
                if (existing == null) return null;

                //existing.CheckinDate = dto.CheckinDate;
                existing.Status = dto.Status;
                //existing.StaffId = dto.StaffId;

                await _checkinRepository.UpdateAsync(existing);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResponseModel.CheckinDto
                {
                    CheckinId = existing.Id,
                    CheckinDate = existing.CheckinDate,
                    Status = existing.Status,
                    StaffId = existing.StaffId,
                    StaffName = existing.Staff.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(id);
                if (existing == null) return null;
                
                await _checkinRepository.SoftDeleteAsync(id);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return "Đã xóa checkin: " + id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<PagedResult<ResponseModel.CheckinDto>> GetCheckinByStaffAsync(Guid staffId, int? pageIndex, int? pageSize )
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var checkin = await _staffcheckinRepository.GetByIdAsync(staffId);
                if (checkin == null) return null;

                var query = _context.Checkins
                    //.Include(c => c.Staff)
                    .Where(p => !p.IsDeleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.CheckinDto
                    {
                        CheckinId = c.Id,
                        CheckinDate = c.CheckinDate,
                        Status = c.Status,
                        StaffId = c.StaffId,
                        StaffName = c.Staff.Name,
                    }).ToListAsync();

                return new PagedResult<ResponseModel.CheckinDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving checkon. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
