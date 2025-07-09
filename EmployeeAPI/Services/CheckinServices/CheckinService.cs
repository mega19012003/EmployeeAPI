using Azure;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Helpers;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Holidays;
using EmployeeAPI.Repositories.LogStatusConfigs;
using EmployeeAPI.Repositories.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Transactions;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static System.Net.Mime.MediaTypeNames;

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
        private readonly ILogStatusConfigRepository _logStatusConfigRepository;
        public CheckinService(ICheckinRepository checkinRepository, ILogStatusConfigRepository logStatusConfigRepository, IUserRepository userRepository, IHolidayRepository holidayRepository, IAllowedIPRepository allowedIPRepository, AppDbContext context, ILogger<CheckinService> logger)
        {
            _checkinRepository = checkinRepository;
            _userRepository = userRepository;
            _holidayRepository = holidayRepository;
            _allowedIPRepository = allowedIPRepository;
            _context = context;
            _logger = logger;
            _logStatusConfigRepository = logStatusConfigRepository;
        }
        
        public async Task<PagedResult<ResponseModel.CheckinResultDto>> GetAllAsync(string? Name, int? Day, int? Month, int? Year, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _checkinRepository.GetAll();

                if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);

                    if (currentUser.DepartmentId == null) throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    var currentDepartmentId = currentUser.DepartmentId;
                    query = query.Where(c => c.Users.DepartmentId == currentDepartmentId);
                }
                else if (currentUserRoles.Contains("Employee"))
                {
                    query = query.Where(c => c.UserId == currentUserId);
                }
                ///////////////////
                var now = DateTime.Now;
                Year ??= now.Year;
                Day ??= now.Day;
                Month ??= now.Month;

                if (Month.HasValue)
                    query = query.Where(c => c.CheckinTime.Month == Month.Value);

                if (Day.HasValue)
                    query = query.Where(c => c.CheckinTime.Day == Day.Value);

                if (Year.HasValue)
                    query = query.Where(c => c.CheckinTime.Year == Year.Value);
                ////////////////////

                if (!string.IsNullOrEmpty(Name))
                {
                    Name = Name.ToLower();
                    query = query.Where(c => c.Users.Fullname.ToLower().Contains(Name));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.CheckinResultDto
                    {
                        CheckinId = c.Id,
                        CheckinTime = c.CheckinTime,
                        CheckoutTime = c.CheckoutTime,
                        LogStatus = (int?)c.LogStatus,
                        Status = c.LogStatus.ToString(),
                        Name = c.Users.Fullname,
                        SalaryPerDay = c.SalaryPerDay,
                    }).ToListAsync();

                return new PagedResult<ResponseModel.CheckinResultDto>
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
        public async Task<ResponseModel.CheckinResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var checkin = await _checkinRepository.GetByIdAsync(id);
                if (checkin == null) throw new ArgumentException("Không tìm thấy thông tin checkin");

                var manager = currentUserRoles.Contains("Manager");
                var employee = currentUserRoles.Contains("Employee");

                if (manager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null) throw new ArgumentException("Không tìm thấy người dùng hiện tại");
                    else if (currentUser.DepartmentId == null) throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");
                    if (checkin.Users.DepartmentId != currentUser.DepartmentId) throw new UnauthorizedAccessException("Manager chỉ có thể truy cập checkin của user cùng phòng ban");
                }
                else if (employee)
                {
                    if (checkin.UserId != currentUserId) throw new UnauthorizedAccessException("Employee chỉ có thể xem checkin của mình");
                }

                return new ResponseModel.CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    CheckinTime = checkin.CheckinTime,
                    CheckoutTime = checkin.CheckoutTime,
                    Name = checkin.Users.Fullname,
                    SalaryPerDay = checkin.SalaryPerDay,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<CheckinResultDto> CheckinAsync(Guid? userId, Guid currentUserId, IList<string> roles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isAdmin = roles.Contains("Administrator");
                var isManager = roles.Contains("Manager");
                var isEmployee = roles.Contains("Employee");

                Guid targetUserId = userId ?? currentUserId;

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng hiện tại");

                if (isManager && currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                var targetUser = await _userRepository.GetActiveUserIdAsync(targetUserId);
                if (targetUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng");

                if (isManager && targetUser.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager chỉ có thể checkin cho user cùng phòng ban");

                if (isEmployee && targetUserId != currentUserId)
                    throw new UnauthorizedAccessException("Employee không thể checkin cho user khác");

                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

                var startOfDay = now.Date;
                var endOfDay = now.Date.AddDays(1).AddTicks(-1);

                var (_, _, currentTime, schedule, _, _) = await GetTimeAndScheduleInfoAsync();

                Enums.LogStatus logStatus;

                if (currentTime < schedule.StartTimeMorning || currentTime > schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime) || (currentTime > schedule.EndTimeMorning && currentTime < schedule.StartTimeAfternoon))
                {
                    throw new ArgumentException("Hiện tại không trong khung giờ cho phép để check-in");
                }

                if (currentTime <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime))
                {
                    logStatus = Enums.LogStatus.OnTime;
                }
                //else if (currentTime <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime + schedule.LateThresholdMinutes))
                //{
                //    logStatus = Enums.LogStatus.Late;
                //}
                else
                {
                    //logStatus = Enums.LogStatus.Absent;
                    logStatus = Enums.LogStatus.Late;
                }

                var existingCheckin = await _context.Checkins
                    .FirstOrDefaultAsync(c => c.UserId == targetUserId && c.CheckinTime >= startOfDay && c.CheckinTime <= endOfDay);

                if (existingCheckin != null)
                    throw new ArgumentException("Đã check-in hôm nay");


                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    CheckinTime = now, 
                    CheckoutTime = DateTime.MinValue,
                    LogStatus = logStatus,
                    SalaryPerDay = 0 
                };

                await _checkinRepository.CreateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    Name = targetUser.Fullname,
                    CheckinTime = checkin.CheckinTime,
                    CheckoutTime = checkin.CheckoutTime,
                    Status = checkin.LogStatus.ToString(),
                    LogStatus = (int?)checkin.LogStatus,
                    SalaryPerDay = checkin.SalaryPerDay
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when checking in");
                throw;
            }
        }

        public async Task<CheckinResultDto> CheckoutAsync(Guid? userId, Guid currentUserId, IList<string> roles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isAdmin = roles.Contains("Administrator");
                var isManager = roles.Contains("Manager");
                var isEmployee = roles.Contains("Employee");

                Guid targetUserId = userId ?? currentUserId;

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng hiện tại");
                if (isManager && currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban");

                var targetUser = await _userRepository.GetActiveUserIdAsync(targetUserId);
                if (targetUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng");

                if (isManager && targetUser.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager chỉ checkout cho user cùng phòng ban");

                if (isEmployee && targetUserId != currentUserId)
                    throw new UnauthorizedAccessException("Employee chỉ checkout cho bản thân");

                var (_, vnTime, currentTime, schedule, _, _) = await GetTimeAndScheduleInfoAsync();

                var startOfDay = vnTime.Date;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                var checkin = await _context.Checkins
                    .FirstOrDefaultAsync(c => c.UserId == targetUserId && c.CheckinTime >= startOfDay && c.CheckinTime <= endOfDay);

                if (checkin == null)
                    throw new ArgumentException("Không tìm thấy bản ghi checkin hôm nay");

                if (checkin.CheckoutTime != DateTime.MinValue)
                    throw new ArgumentException("Đã checkout rồi");

                checkin.CheckoutTime = vnTime; // lưu giờ VN
                double overtimeHours = 0;

                var workEndTime = schedule.EndTimeAfternoon;

                if (currentTime > workEndTime.AddMinutes(schedule.LogAllowtime))
                {
                    checkin.LogStatus = Enums.LogStatus.Overtime;
                    overtimeHours = (currentTime - workEndTime).TotalHours;
                }
                else if (currentTime >= workEndTime && currentTime <= workEndTime.AddMinutes(schedule.LogAllowtime) && checkin.LogStatus == Enums.LogStatus.OnTime)
                {
                    checkin.LogStatus = Enums.LogStatus.OnTime;
                }
                else 
                {
                    checkin.LogStatus = Enums.LogStatus.LeaveEarly;
                }

                var totalWorkedHours = (checkin.CheckoutTime - checkin.CheckinTime).TotalHours;

                var lunchBreak = (schedule.StartTimeAfternoon - schedule.EndTimeMorning).TotalHours;

                double normalWorkedHours;
                if (checkin.CheckinTime.TimeOfDay < schedule.EndTimeMorning.ToTimeSpan() && checkin.CheckoutTime.TimeOfDay > schedule.StartTimeAfternoon.ToTimeSpan())
                {
                    normalWorkedHours = Math.Floor(totalWorkedHours - lunchBreak);
                }
                else
                {
                    normalWorkedHours = Math.Floor(totalWorkedHours);
                }

                if (normalWorkedHours < 0) normalWorkedHours = 0;

                checkin.SalaryPerDay = await CalculateSalaryPerDayNew(targetUser, normalWorkedHours, overtimeHours, checkin.LogStatus);

                await _checkinRepository.UpdateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    Name = targetUser.Fullname,
                    CheckinTime = checkin.CheckinTime,
                    CheckoutTime = checkin.CheckoutTime,
                    LogStatus = (int?)checkin.LogStatus,
                    Status = checkin.LogStatus.ToString(),
                    SalaryPerDay = checkin.SalaryPerDay
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when checking out");
                throw;
            }
        }

        //private async Task<(DateTime nowUtc, DateTime vnTime, TimeOnly currentTime, ScheduleTime schedule, bool isHoliday, bool isSunday)> GetTimeAndScheduleInfoAsync()
        //{
        //    var nowUtc = DateTime.UtcNow;
        //    var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        //    var vnTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);
        //    var currentTime = TimeOnly.FromDateTime(vnTime);

        //    var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
        //    if (schedule == null) throw new Exception("không thể tìm thấy thời gian làm việc");

        //    var isHoliday = await _holidayRepository.IsHolidayAsync(nowUtc);
        //    //var isHoliday = await _holidayRepository.IsHolidayAsync(DateOnly.FromDateTime(nowUtc));
        //    var isSunday = vnTime.DayOfWeek == DayOfWeek.Sunday;

        //    return (nowUtc, vnTime, currentTime, schedule, isHoliday, isSunday);
        //}
        private async Task<(DateTime nowUtc, DateTime vnTime, TimeOnly currentTime, ScheduleTime schedule, bool isHoliday, bool isSunday)> GetTimeAndScheduleInfoAsync()
        {
            var nowUtc = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);
            var currentTime = TimeOnly.FromDateTime(vnTime);

            var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
            if (schedule == null) throw new Exception("Không thể tìm thấy thời gian làm việc");

            var isHoliday = await _holidayRepository.IsHolidayAsync(vnTime);
            var isSunday = vnTime.DayOfWeek == DayOfWeek.Sunday;

            return (nowUtc, vnTime, currentTime, schedule, isHoliday, isSunday);
        }
        public async Task<ResponseModel.CheckinResultDto> UpdateAsync(ResponseModel.UpdateCheckinDto dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(dto.CheckinId);
                if (existing == null)
                    throw new ArgumentException("Không tìm thấy bản ghi checkin này");

                var employee = await _userRepository.GetActiveUserIdAsync(existing.UserId);
                if (employee == null)
                    throw new ArgumentException("Không tìm thấy người dùng");

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new UnauthorizedAccessException("Manager chỉ có thể cập nhật checkin cho user cùng phòng ban");
                }

                var (_, vnTime, currentTime, schedule, _, _) = await GetTimeAndScheduleInfoAsync();

                double overtimeHours = 0;
                var endTimeAfternoon = schedule.EndTimeAfternoon; // TimeOnly
                if (currentTime > endTimeAfternoon)
                {
                    overtimeHours = (currentTime - endTimeAfternoon).TotalHours;
                }

                double lunchBreakHours = (schedule.StartTimeAfternoon - schedule.EndTimeMorning).TotalHours;

                double totalWorkedHours = (existing.CheckoutTime - existing.CheckinTime).TotalHours;
                var normalWorkedHours = 0.0;
                if (existing.CheckinTime.TimeOfDay < schedule.EndTimeMorning.ToTimeSpan() && existing.CheckoutTime.TimeOfDay > schedule.StartTimeAfternoon.ToTimeSpan())
                {
                    normalWorkedHours = Math.Floor(totalWorkedHours - lunchBreakHours);
                }
                else
                {
                    normalWorkedHours = Math.Floor(totalWorkedHours);
                }

                if (normalWorkedHours < 0) normalWorkedHours = 0;

                existing.LogStatus = dto.LogStatus;

                double salaryPerDay = await CalculateSalaryPerDayNew(employee, normalWorkedHours, overtimeHours, dto.LogStatus);
                existing.SalaryPerDay = salaryPerDay;

                await _checkinRepository.UpdateAsync(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CheckinResultDto
                {
                    CheckinId = existing.Id,
                    Name = employee.Fullname,
                    CheckinTime = existing.CheckinTime,
                    CheckoutTime = existing.CheckoutTime,
                    LogStatus = (int?)existing.LogStatus,
                    Status = existing.LogStatus.ToString(),
                    SalaryPerDay = existing.SalaryPerDay
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when updating checkin");
                throw;
            }
        }

        public async Task<double> CalculateSalaryPerDayNew(User user, double normalHours, double overtimeHours, Enums.LogStatus logStatus)
        {
            var logStatusConfigs = await _logStatusConfigRepository.GetAllAsync();
            double onTimeMultiplier = 1;
            double overtimeMultiplier = 1;

            foreach (var item in logStatusConfigs)
            {
                if (item.Id == (int)Enums.LogStatus.OnTime) onTimeMultiplier = item.SalaryMultiplier;
                if (item.Id == (int)Enums.LogStatus.Overtime) overtimeMultiplier = item.SalaryMultiplier;
                if (item.Id == (int)logStatus) onTimeMultiplier = item.SalaryMultiplier; 
            }

            double salary = 0;

            if (logStatus == Enums.LogStatus.Overtime)
            {
                salary = (normalHours * user.SalaryPerHour * onTimeMultiplier) + (overtimeHours * user.SalaryPerHour * overtimeMultiplier);
            }
            else
            {
                salary = normalHours * user.SalaryPerHour * onTimeMultiplier;
            }
            salary = Math.Floor(salary);
            return salary;
        }

        public async Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(id);
                if (existing == null) throw new ArgumentException("không thể tìm thấy bản ghi checkin này");

                var employee = await _userRepository.GetUserInfoAsync(existing.UserId);
                if (employee == null) throw new ArgumentException("Không tìm thấy user trong bản ghi checkin");

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);

                if (currentUserRoles.Contains("Administrator"))
                {
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null) throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (currentUser.DepartmentId != employee.DepartmentId) throw new UnauthorizedAccessException("Manager chỉ có thể xóa checkin của user cùng phòng ban");
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
        public async Task<PagedResult<ResponseModel.CheckinDetailDto>> GetCheckinByUserAsync(Guid currentUserId, IList<string> currentUserRoles, Guid? staffId, int? Day, int? Month, int? Year, int? pageIndex, int? pageSize)
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

                    if (currentUser.DepartmentId == null) throw new Exception("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (staffId == null || staffId == Guid.Empty) throw new ArgumentException("Vui lòng chọn user");

                    var findUser = await _userRepository.GetUserInfoAsync(staffId.Value);
                    if (findUser == null) throw new ArgumentException("Không thể tìm thấy user này");

                    if (findUser.DepartmentId != currentUser.DepartmentId) throw new UnauthorizedAccessException("Manager chỉ có thể xem danh sách checkin từ user cùng phòng ban");

                }
                else if (currentUserRoles.Contains("Administrator"))
                {
                    if (staffId == null || staffId == Guid.Empty) throw new ArgumentException("Vui lòng chọn user");
                }

                pageIndex ??= 1;
                pageSize ??= 10;
                var now = DateTime.Now;
                Year ??= now.Year;
                Day ??= now.Day;
                Month ??= now.Month;

                var user = await _userRepository.GetUserInfoAsync(staffId.Value);

                if (user == null)
                    throw new ArgumentException("Không thể tìm thấy user này");

                var query = _context.Checkins.Where(p => !p.IsDeleted && p.UserId == staffId.Value);
                query = query.Where(c => c.CheckinTime.Year == Year.Value);

                if (Month.HasValue)
                    query = query.Where(c => c.CheckinTime.Month == Month.Value);

                if (Day.HasValue)
                    query = query.Where(c => c.CheckinTime.Day == Day.Value);

                if (Year.HasValue)
                    query = query.Where(c => c.CheckinTime.Year == Year.Value);

                var totalCount = await query.CountAsync();

                var itemsRaw = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Include(c => c.Users) 
                    .ToListAsync();

                var items = new List<ResponseModel.CheckinDetailDto>();

                foreach (var c in itemsRaw)
                {
                    items.Add(new ResponseModel.CheckinDetailDto
                    {
                        Id = c.Id,
                        CheckinTime = c.CheckinTime,
                        CheckoutTime = c.CheckoutTime,
                        LogStatus = (int?)c.LogStatus,
                        Status = c.LogStatus.ToString(),
                        Name = c.Users.Fullname,
                        SalaryPerDay = c.SalaryPerDay,
                    });
                }

                return new PagedResult<ResponseModel.CheckinDetailDto>
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
