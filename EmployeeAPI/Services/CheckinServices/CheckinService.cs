using System.Runtime.CompilerServices;
using System.Transactions;
using Azure;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                var existUsers = await _userRepository.GetByIdAsync(dto.userId);
                if (existUsers == null)
                    throw new ArgumentException("Cannot find Users id");

                // 2. Lấy múi giờ VN
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                // 3. Xử lý thời gian check-in:
                // Nếu client gửi thời gian rồi (giờ VN) thì convert sang UTC để lưu DB
                // Nếu không thì lấy giờ hiện tại UTC để lưu
                DateTime utcCheckinDate;
                DateTime vnCheckinDate;

                if (dto.CheckinDate.HasValue)
                {
                    // dto.CheckinDate giả sử là giờ VN
                    vnCheckinDate = dto.CheckinDate.Value;

                    // Chuyển giờ VN sang UTC để lưu
                    utcCheckinDate = TimeZoneInfo.ConvertTimeToUtc(vnCheckinDate, vnTimeZone);
                }
                else
                {
                    // Nếu không nhập thì lấy giờ hiện tại
                    utcCheckinDate = DateTime.UtcNow;

                    // Chuyển UTC về VN để tính toán status
                    vnCheckinDate = TimeZoneInfo.ConvertTimeFromUtc(utcCheckinDate, vnTimeZone);
                }

                // 4. Kiểm tra đã check-in ngày đó chưa (theo giờ VN)
                var alreadyCheckedIn = _context.Checkins
                    .Where(c => c.UserId == dto.userId)
                    .AsEnumerable()
                    .Any(c =>
                        TimeZoneInfo.ConvertTimeFromUtc(c.CheckinDate, vnTimeZone).Date == vnCheckinDate.Date
                    );

                if (alreadyCheckedIn)
                    throw new ArgumentException("Bạn đã check-in ngày này rồi.");

                // 5. Lấy giờ làm việc trong bảng cấu hình
                var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
                if (schedule == null)
                    throw new Exception("Chưa thiết lập giờ làm việc");

                // 6. Tính thời gian muộn
                var lateTime = schedule.StartTime.AddMinutes(schedule.LateThresholdMinutes);

                // Giờ hiện tại (theo VN) dưới dạng TimeOnly để so sánh với giờ bắt đầu
                var currentTimeOnly = TimeOnly.FromDateTime(vnCheckinDate);

                // 7. Xác định trạng thái OnTime hay Late
                var status = currentTimeOnly <= lateTime ? CheckinStatus.OnTime : CheckinStatus.Late;

                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    CheckinDate = utcCheckinDate,
                    Status = status,
                    UserId = dto.userId,
                };

                await _checkinRepository.CreateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CheckinDto
                {
                    CheckinId = checkin.Id,
                    CheckinDate = checkin.CheckinDate,
                    CheckinStatus = checkin.Status,
                    Status = checkin.Status.ToString(),
                    userId = checkin.UserId,
                    Name = existUsers.Fullname
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error while creating checkin");
                throw;
            }
        }

        public async Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto, Guid currentUserId, IList<string> currentUserRoles)
        {
 
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(dto.CheckinId);
                if (existing == null)
                    throw new ArgumentException("Cannot find checkin id");

                var employee = await _userRepository.GetByIdAsync(existing.UserId);
                if (employee == null)
                    throw new ArgumentException("Cannot find employee for this checkin");

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                // Kiểm tra quyền
                if (currentUserRoles.Contains("Administrator"))
                {
                    // Admin: được quyền update mọi checkin
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    // Manager: chỉ được update checkin nhân viên trong cùng phòng ban
                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot update checkin made by employeee from other department");
                }
                else
                {
                    // Người dùng khác không có quyền update checkin
                    throw new UnauthorizedAccessException("Bạn không có quyền cập nhật checkin");
                }

                existing.Status = dto.CheckinStatus;

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
                    Name = employee.Fullname,
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(id);
                if (existing == null)
                    throw new ArgumentException("Cannot find checkin id");

                var employee = await _userRepository.GetByIdAsync(existing.UserId);
                if (employee == null)
                    throw new ArgumentException("Cannot find employee for this checkin");

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUserRoles.Contains("Administrator"))
                {
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot delete checkin from other department");
                }
                else
                {
                    throw new UnauthorizedAccessException("Access Denied");
                }

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

        public async Task<PagedResult<ResponseModel.CheckinDto>> GetCheckinByUserAsync(Guid currentUserId, IList<string> currentUserRoles, Guid? staffId, int? pageIndex, int? pageSize)
        {
            try
            {
                // Gán ngầm staffId nếu user là employee
                if (!currentUserRoles.Contains("Administrator") && !currentUserRoles.Contains("Manager"))
                {
                    staffId = currentUserId;
                }
                else
                {
                    // Nếu admin hoặc manager thì staffId phải có giá trị
                    if (staffId == null || staffId == Guid.Empty)
                        throw new ArgumentException("Please input staffId");
                }

                pageIndex ??= 1;
                pageSize ??= 10;

                // Kiểm tra user được lấy có tồn tại không
                var user = await _userRepository.GetByIdAsync(staffId.Value);
                if (user == null)
                    throw new ArgumentException("Cannot find user id");

                // Manager chỉ lấy được dữ liệu trong phòng ban của mình
                if (currentUserRoles.Contains("Manager") && user.DepartmentId != (await _userRepository.GetByIdAsync(currentUserId)).DepartmentId)
                    throw new UnauthorizedAccessException("Manager cannot access checkins from other departments");

                var query = _context.Checkins
                    .Where(p => !p.IsDeleted && p.UserId == staffId.Value);

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
                _logger.LogError(ex, "Error occurred while deleting checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }


        //public async Task<PagedResult<ResponseModel.CheckinDto>> GetCheckinByUserAsync(Guid userId, int? pageIndex, int? pageSize )
        //{
        //    try
        //    {
        //        pageIndex ??= 1;
        //        pageSize ??= 10;

        //        var checkin = await _userRepository.GetByIdAsync(userId);
        //        if (checkin == null)
        //            throw new ArgumentException("Cannot find Users id");

        //        var query = _context.Checkins
        //            //.Include(c => c.Users)
        //            .Where(p => !p.IsDeleted && p.UserId == userId);

        //        var totalCount = await query.CountAsync();

        //        var items = await query
        //            .Skip((pageIndex.Value - 1) * pageSize.Value)
        //            .Take(pageSize.Value)
        //            .Select(c => new ResponseModel.CheckinDto
        //            {
        //                CheckinId = c.Id,
        //                CheckinDate = c.CheckinDate,
        //                CheckinStatus = c.Status,
        //                Status = c.Status.ToString(),
        //                userId = c.UserId,
        //                Name = c.Users.Fullname,
        //            }).ToListAsync();

        //        return new PagedResult<ResponseModel.CheckinDto>
        //        {
        //            Items = items,
        //            PageIndex = pageIndex.Value,
        //            PageSize = pageSize.Value,
        //            TotalCount = totalCount
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while retrieving checkon. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
        //        throw;
        //    }
        //}
    }
}
