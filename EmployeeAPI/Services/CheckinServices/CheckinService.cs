using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Transactions;
using Azure;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Helpers;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Holidays;
using EmployeeAPI.Repositories.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;

namespace EmployeeAPI.Services.CheckinServices
{
    public class CheckinService : ICheckinService
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHolidayRepository _holidayRepository;
        private readonly IAllowedIPRepository _allowedIPRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<CheckinService> _logger;

        public CheckinService(ICheckinRepository checkinRepository, IUserRepository userRepository, IHolidayRepository holidayRepository, IAllowedIPRepository allowedIPRepository, AppDbContext context, ILogger<CheckinService> logger)
        {
            _checkinRepository = checkinRepository;
            _userRepository = userRepository;
            _holidayRepository = holidayRepository;
            _allowedIPRepository = allowedIPRepository;
            _context = context;
            _logger = logger;
        }
        
        public async Task<PagedResult<ResponseModel.CheckinDto>> GetAllAsync(string? Name, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _checkinRepository.GetAll();

                if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);

                    if (currentUser.DepartmentId == null) throw new Exception("Manager does not belong to any department");

                    var currentDepartmentId = currentUser.DepartmentId;
                    query = query.Where(c => c.Users.DepartmentId == currentDepartmentId);
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    Name = Name.ToLower();
                    query = query.Where(c => c.Users.Fullname.ToLower().Contains(Name));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.CheckinDto
                    {
                        CheckinId = c.Id,
                        CheckinDate = c.CheckinDate,
                        CheckinStatus = c.CheckinStatus,
                        Checkin = c.CheckinStatus.ToString(),
                        CheckoutDate = c.CheckoutDate,
                        CheckoutStatus = c.CheckoutStatus,
                        Checkout = c.CheckoutStatus.ToString(),
                        userId = c.UserId,
                        Name = c.Users.Fullname,
                        SalaryPerDay = c.SalaryPerDay,
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
                    CheckinId = c.Id,
                    CheckinDate = c.CheckinDate,
                    CheckinStatus = c.CheckinStatus,
                    Checkin = c.CheckinStatus.ToString(),
                    CheckoutDate = c.CheckoutDate,
                    CheckoutStatus = c.CheckoutStatus,
                    Checkout = c.CheckoutStatus.ToString(),
                    userId = c.UserId,
                    Name = c.Users.Fullname,
                    SalaryPerDay = c.SalaryPerDay,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<CheckinDto> CreateAsync(CreateCheckin dto, Guid currentUserId, IList<string> roles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isAdmin = roles.Contains("Admin");
                var isManager = roles.Contains("Manager");
                var isEmployee = roles.Contains("Employee");

                Guid targetUserId;
                targetUserId = currentUserId;

                if (isAdmin)
                {
                    targetUserId = dto.userId == null || dto.userId == Guid.Empty ? currentUserId : dto.userId.Value;
                }
                else if (isManager)
                {
                    targetUserId = dto.userId == null || dto.userId == Guid.Empty ? currentUserId : dto.userId.Value;

                    var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                    var targetUser = await _userRepository.GetByIdAsync(targetUserId);

                    if (currentUser == null || targetUser == null) throw new ArgumentException("User not found");

                    if (currentUser.DepartmentId != targetUser.DepartmentId) throw new ArgumentException("Manager can only checkin for users in the same department");
                }
                else
                {
                    throw new UnauthorizedAccessException("Access Denied");
                }

                var user = await _userRepository.GetByIdAsync(targetUserId);
                if (user == null) throw new ArgumentException("User not found");

                var nowUtc = DateTime.UtcNow;
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);

                // Kiểm tra đã checkin trong ngày chưa (theo giờ Việt Nam nhưng lưu UTC)
                var startOfDay = new DateTime(vnTime.Year, vnTime.Month, vnTime.Day, 0, 0, 0);
                var endOfDay = new DateTime(vnTime.Year, vnTime.Month, vnTime.Day, 23, 59, 59);
                var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDay, vnTimeZone);
                var endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDay, vnTimeZone);

                var alreadyCheckedIn = await _context.Checkins.AnyAsync(c =>
                    c.UserId == targetUserId &&
                    c.CheckinDate >= startOfDayUtc &&
                    c.CheckinDate <= endOfDayUtc
                );

                if (alreadyCheckedIn) throw new InvalidOperationException("User has already checked in today");

                // Tính trạng thái check-in
                var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
                if (schedule == null) throw new Exception("Schedule not found");

                var isSunday = vnTime.DayOfWeek == DayOfWeek.Sunday;
                var isHoliday = await _holidayRepository.IsHolidayAsync(nowUtc);

                var checkinStatus = isSunday || isHoliday
                    ? CheckinStatus.Overtime : (
                        TimeOnly.FromDateTime(vnTime) > schedule.EndTime.AddMinutes(schedule.LateThresholdMinutes)
                            ? CheckinStatus.Overtime : (
                                TimeOnly.FromDateTime(vnTime) <= schedule.StartTime.AddMinutes(schedule.LateThresholdMinutes) ? CheckinStatus.OnTime : CheckinStatus.Late
                            )
                      );

                if (isEmployee)
                {
                    dto.CheckinStatus = checkinStatus;
                }

                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    CheckinDate = nowUtc,
                    CheckinStatus = dto.CheckinStatus ?? checkinStatus,
                    CheckoutStatus = dto.CheckinStatus ?? CheckinStatus.Absent
                };

                await _checkinRepository.CreateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinDto
                {
                    CheckinId = checkin.Id,
                    CheckinDate = checkin.CheckinDate,
                    CheckinStatus = checkin.CheckinStatus,
                    CheckoutStatus = checkin.CheckoutStatus,
                    userId = targetUserId,
                    Name = user.Fullname
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when creating checkin");
                throw;
            }
        }


        public async Task<ResponseModel.CheckinDto> CheckoutAsync(ResponseModel.CreateCheckout dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isAdmin = currentUserRoles.Contains("Admin");
                var isManager = currentUserRoles.Contains("Manager");
                var isEmployee = currentUserRoles.Contains("Employee");

                Guid targetUserId;

                if (isAdmin)
                {
                    targetUserId = !dto.userId.HasValue || dto.userId == Guid.Empty ? currentUserId : dto.userId.Value;
                }
                else if (isManager)
                {
                    targetUserId = !dto.userId.HasValue || dto.userId == Guid.Empty ? currentUserId : dto.userId.Value;

                    var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                    var targetUser = await _userRepository.GetByIdAsync(targetUserId);

                    if (currentUser.DepartmentId != targetUser.DepartmentId) throw new ArgumentException("Manager can only checkout for user in the same department");
                }
                else if (isEmployee)
                {
                    targetUserId = currentUserId;
                }
                else
                {
                    throw new Exception("");
                }

                var existUser = await _userRepository.GetByIdAsync(targetUserId);
                if (existUser == null) throw new ArgumentException("User not found");

                // Giờ hệ thống theo VN (ép ngầm)
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

                // Tìm check-in hôm nay chưa checkout
                var checkins = await _context.Checkins.Where(c => c.UserId == targetUserId && !c.IsDeleted && c.CheckoutDate == default).ToListAsync();

                var todayCheckin = checkins.FirstOrDefault(c =>
                {
                    var checkinDate = c.CheckinDate;
                    if (checkinDate.Kind == DateTimeKind.Unspecified)
                        checkinDate = DateTime.SpecifyKind(checkinDate, DateTimeKind.Utc);

                    var vnDate = checkinDate.Kind == DateTimeKind.Utc ? TimeZoneInfo.ConvertTimeFromUtc(checkinDate, vnTimeZone) : TimeZoneInfo.ConvertTime(checkinDate, vnTimeZone);

                    return vnDate.Date == nowVn.Date;
                });

                if (todayCheckin == null) throw new ArgumentException("You haven't checkin today or already checkout");

                var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
                if (schedule == null) throw new Exception("Work schedule time hasn't been set");

                var currentTimeOnly = TimeOnly.FromDateTime(nowVn);
                var overtimeThreshold = schedule.EndTime.AddMinutes(schedule.LateThresholdMinutes);

                CheckinStatus newStatus;

                if (currentTimeOnly > overtimeThreshold)
                    newStatus = CheckinStatus.Overtime;
                else if (currentTimeOnly < schedule.EndTime)
                    newStatus = CheckinStatus.LeaveEarly;
                else
                    newStatus = CheckinStatus.OnTime;

                var checkoutUtc = TimeZoneInfo.ConvertTimeToUtc(nowVn, vnTimeZone);

                todayCheckin.CheckoutDate = checkoutUtc;
                todayCheckin.CheckoutStatus = newStatus;

                todayCheckin.SalaryPerDay = await CalculateSalaryPerDay.CalculateSalaryPerDayAsync(_context, existUser, todayCheckin.CheckinStatus, todayCheckin.CheckoutStatus);

                _context.Checkins.Update(todayCheckin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CheckinDto
                {
                    CheckinId = todayCheckin.Id,
                    CheckinDate = todayCheckin.CheckinDate,
                    CheckinStatus = todayCheckin.CheckinStatus,
                    Checkin = todayCheckin.CheckinStatus.ToString(),
                    CheckoutDate = todayCheckin.CheckoutDate,
                    CheckoutStatus = todayCheckin.CheckoutStatus,
                    Checkout = todayCheckin.CheckoutStatus.ToString(),
                    userId = todayCheckin.UserId,
                    Name = existUser.Fullname,
                    SalaryPerDay = todayCheckin.SalaryPerDay
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when checkout");
                throw;
            }
        }

        public async Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto, Guid currentUserId, IList<string> currentUserRoles)
        {
 
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(dto.CheckinId);
                if (existing == null) throw new ArgumentException("Checkin not found");

                var employee = await _userRepository.GetByIdAsync(existing.UserId);
                if (employee == null) throw new ArgumentException("User not found");

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId != employee.DepartmentId) throw new UnauthorizedAccessException("Manager cannot update checkin made by employeee from other department");

                    if (currentUser.DepartmentId == null) throw new Exception("Manager does not belong to any department");
                }

                existing.CheckinStatus = dto.CheckinStatus;
                existing.CheckoutStatus = dto.CheckoutStatus;
                existing.SalaryPerDay = await CalculateSalaryPerDay.CalculateSalaryPerDayAsync(_context, employee, existing.CheckinStatus, existing.CheckoutStatus);

                await _checkinRepository.UpdateAsync(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CheckinDto
                {
                    CheckinId = existing.Id,
                    CheckinDate = existing.CheckinDate,
                    CheckinStatus = existing.CheckinStatus,
                    Checkin = existing.CheckinStatus.ToString(),
                    CheckoutDate = existing.CheckoutDate,
                    CheckoutStatus = existing.CheckoutStatus,
                    Checkout = existing.CheckoutStatus.ToString(),
                    userId = existing.UserId,
                    Name = employee.Fullname,
                    SalaryPerDay = existing.SalaryPerDay,
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AutoMarkAbsentAsync(TimeOnly endTime)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var today = vnNow.Date;

            if (vnNow.DayOfWeek == DayOfWeek.Sunday)
            {
                _logger.LogInformation("Sunday, no checkin");
                return;
            }

            bool isHoliday = await _holidayRepository.IsHolidayAsync(today);
            if (isHoliday)
            {
                _logger.LogInformation("today is a holiday, no marking absent");
                return;
            }

            if (TimeOnly.FromDateTime(vnNow) < endTime)
            {
                _logger.LogInformation("Not work end time, no marking absent");
                return;
            }

            // Tính thời gian bắt đầu và kết thúc ngày theo UTC dựa trên VN timezone
            var vnTodayStart = vnNow.Date; 
            var vnTodayStartUtc = TimeZoneInfo.ConvertTimeToUtc(vnTodayStart, vnTimeZone);
            var vnTodayEndUtc = vnTodayStartUtc.AddDays(1);

            // Lấy checkin trong khoảng thời gian này (UTC)
            var checkinsInRange = await _context.Checkins
                .Where(c => c.CheckinDate >= vnTodayStartUtc && c.CheckinDate < vnTodayEndUtc)
                .ToListAsync();

            // Lọc checkin đúng ngày theo VN timezone
            var checkedInUserIds = checkinsInRange
                .Where(c => TimeZoneInfo.ConvertTimeFromUtc(c.CheckinDate, vnTimeZone).Date == today)
                .Select(c => c.UserId).Distinct().ToList();

            var allUsers = await _userRepository.GetAll().ToListAsync();

            var absentUsers = allUsers.Where(u => !checkedInUserIds.Contains(u.UserId)).ToList();

            foreach (var user in absentUsers)
            {
                double salary = await CalculateSalaryPerDay.CalculateSalaryPerDayAsync(_context, user, CheckinStatus.Absent, CheckinStatus.Absent);
                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    CheckinStatus = CheckinStatus.Absent,
                    CheckinDate = DateTime.UtcNow,
                    CheckoutStatus = CheckinStatus.Absent,
                    CheckoutDate = DateTime.UtcNow,
                    SalaryPerDay = salary
                };

                await _checkinRepository.CreateAsync(checkin);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Mark {absentUsers.Count} users absent on {today:dd/MM/yyyy}.");
        }

        public async Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(id);
                if (existing == null) throw new ArgumentException("Cannot find checkin");

                var employee = await _userRepository.GetByIdAsync(existing.UserId);
                if (employee == null) throw new ArgumentException("Cannot find employee for this checkin");

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);

                if (currentUserRoles.Contains("Administrator"))
                {
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null) throw new Exception("Manager does not belong to any department");

                    if (currentUser.DepartmentId != employee.DepartmentId) throw new UnauthorizedAccessException("Manager cannot delete checkin from other department");
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
                if (!currentUserRoles.Contains("Administrator") && !currentUserRoles.Contains("Manager"))
                {
                    staffId = currentUserId;
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);

                    if (currentUser.DepartmentId == null) throw new Exception("Manager does not belong to any department");

                    if (staffId == null || staffId == Guid.Empty) throw new ArgumentException("Please input userid");

                    var findUser = await _userRepository.GetByIdAsync(staffId.Value);
                    if (findUser == null) throw new ArgumentException("Cannot find user id");

                    if (findUser.DepartmentId != currentUser.DepartmentId) throw new UnauthorizedAccessException("Manager cannot access checkins from other departments");

                }
                else if (currentUserRoles.Contains("Administrator"))
                {
                    if (staffId == null || staffId == Guid.Empty) throw new ArgumentException("Please input user id");
                }

                pageIndex ??= 1;
                pageSize ??= 10;
                
                var user = await _userRepository.GetByIdAsync(staffId.Value);
                if (user == null)
                    throw new ArgumentException("Cannot find user id");

                var query = _context.Checkins.Where(p => !p.IsDeleted && p.UserId == staffId.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.CheckinDto
                    {
                        CheckinId = c.Id,
                        CheckinDate = c.CheckinDate,
                        CheckinStatus = c.CheckinStatus,
                        Checkin = c.CheckinStatus.ToString(),
                        CheckoutDate = c.CheckoutDate,
                        CheckoutStatus = c.CheckoutStatus,
                        Checkout = c.CheckoutStatus.ToString(),
                        userId = c.UserId,
                        Name = c.Users.Fullname,
                        SalaryPerDay = c.SalaryPerDay,
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
    }
}
