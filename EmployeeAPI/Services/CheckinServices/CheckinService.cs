using System.Runtime.CompilerServices;
using System.Transactions;
using Azure;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;

namespace EmployeeAPI.Services.CheckinServices
{
    public class CheckinService : ICheckinService
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<CheckinService> _logger;

        public CheckinService(ICheckinRepository checkinRepository, IAuthRepository authRepository, IUserRepository userRepository, AppDbContext context, ILogger<CheckinService> logger)
        {
            _checkinRepository = checkinRepository;
            _userRepository = userRepository;
            _authRepository = authRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ResponseModel.CheckinDto>> GetAllAsync(string? Name, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _context.Checkins
                    .Include(c => c.Users)
                    .Where(f => string.IsNullOrEmpty(Name) || f.Users.Fullname.ToLower().Contains(Name.ToLower()))
                    .Where(p => !p.IsDeleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.CheckinDto
                    {
                        CheckinId = c.Id,
                        CheckinDate = c.CheckinDate,
                        CheckinStatus = c.Status,
                        Status = c.Status.ToString(),
                        userId = c.UserId,
                        Name = c.Users.Fullname,
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
                    CheckinStatus = c.Status,
                    Status = c.Status.ToString(),
                    userId = c.UserId,
                    Name = c.Users.Fullname,
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
                /*if (dto.userId == Guid.Empty)
                    throw new ArgumentException("Users id cannot be empty");*/

                var existUsers = await _userRepository.GetByIdAsync(dto.userId);
                if (existUsers == null)
                    throw new ArgumentException("Cannot find Users id");

                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    CheckinDate = dto.CheckinDate,
                    Status = dto.CheckinStatus,
                    UserId = dto.userId,
                };

                await _checkinRepository.CreateAsync(checkin);
                await _context.SaveChangesAsync(); 
                await transaction.CommitAsync();

                var Users = await _userRepository.GetByIdAsync(dto.userId);
                return new ResponseModel.CheckinDto
                {
                    CheckinId = checkin.Id,
                    CheckinDate = checkin.CheckinDate,
                    CheckinStatus = checkin.Status,
                    Status = checkin.Status.ToString(),
                    userId = checkin.UserId,
                    Name = Users.Fullname,
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
                if (existing == null)
                    throw new ArgumentException("Cannot find checkin id");

                //existing.CheckinDate = dto.CheckinDate;
                existing.Status = dto.CheckinStatus;
                //existing.userId = dto.userId;

                await _checkinRepository.UpdateAsync(existing);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResponseModel.CheckinDto
                {
                    CheckinId = existing.Id,
                    CheckinDate = existing.CheckinDate,
                    CheckinStatus = existing.Status,
                    Status = existing.Status.ToString(),
                    userId = existing.UserId,
                    Name = existing.Users.Fullname,
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
                if (existing == null)
                    throw new ArgumentException("Cannot find checkin id");

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

        public async Task<PagedResult<ResponseModel.CheckinDto>> GetCheckinByUserAsync(Guid userId, int? pageIndex, int? pageSize )
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var checkin = await _userRepository.GetByIdAsync(userId);
                if (checkin == null)
                    throw new ArgumentException("Cannot find Users id");

                var query = _context.Checkins
                    //.Include(c => c.Users)
                    .Where(p => !p.IsDeleted && p.UserId == userId);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.CheckinDto
                    {
                        CheckinId = c.Id,
                        CheckinDate = c.CheckinDate,
                        CheckinStatus = c.Status,
                        Status = c.Status.ToString(),
                        userId = c.UserId,
                        Name = c.Users.Fullname,
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
