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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CheckinService(ICheckinRepository checkinRepository, ILogStatusConfigRepository logStatusConfigRepository, IUserRepository userRepository, IHolidayRepository holidayRepository, IAllowedIPRepository allowedIPRepository, AppDbContext context, ILogger<CheckinService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _checkinRepository = checkinRepository;
            _userRepository = userRepository;
            _holidayRepository = holidayRepository;
            _allowedIPRepository = allowedIPRepository;
            _context = context;
            _logger = logger;
            _logStatusConfigRepository = logStatusConfigRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<PagedResult<ResponseModel.CheckinResultDto>> GetAllAsync(string? Name, Guid? companyId, Guid? departmentId, Guid? positionId, int? Day, int? Month, int? Year, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _checkinRepository.GetAll();

                if (currentUserRoles.Contains("Administrator"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Administrator chưa có công ty.");

                    // Lọc theo công ty của user checkin
                    query = query.Where(c => c.Users.CompanyId == currentUser.CompanyId);
                }
                else if (currentUserRoles.Contains("Manager"))
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

                // Lọc theo năm 
                if (Year.HasValue)
                    query = query.Where(c => c.CheckinTime.Year == Year.Value);

                // Chỉ lọc theo tháng nếu được truyền
                if (Month.HasValue)
                    query = query.Where(c => c.CheckinTime.Month == Month.Value);

                // Chỉ lọc theo ngày nếu được truyền
                if (Day.HasValue)
                    query = query.Where(c => c.CheckinTime.Day == Day.Value);
                ////////////////////

                if (!string.IsNullOrEmpty(Name))
                {
                    Name = Name.ToLower();
                    query = query.Where(c => c.Users.Fullname.ToLower().Contains(Name));
                }

                if (companyId.HasValue)
                {
                    query = query.Where(c => c.Users.CompanyId == companyId);
                }

                if (departmentId.HasValue)
                {
                    query = query.Where(c => c.Users.DepartmentId == departmentId);
                }

                if (positionId.HasValue)
                {
                    query = query.Where(c => c.Users.PositionId == positionId);
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
                        UserId = c.UserId,
                        DeviceInfo = c.DeviceInfo ?? "",
                        CheckinIP = c.CheckinIP,
                        CheckoutIP = c.CheckoutIP,
                        TotalTime = c.TotalTime
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

                var isAdmin = currentUserRoles.Contains("Administrator");
                var manager = currentUserRoles.Contains("Manager");
                var employee = currentUserRoles.Contains("Employee");

                if (isAdmin)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Administrator chưa có công ty.");
                    else if (checkin.Users.CompanyId != currentUser.CompanyId)
                        throw new ArgumentException("Administrator chỉ có thể xem checkin của user trong công ty của mình.");
                }
                else if (manager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser.DepartmentId == null) throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");
                    else if (checkin.Users.DepartmentId != currentUser.DepartmentId) throw new ArgumentException("Manager chỉ có thể truy cập checkin của user cùng phòng ban");
                }
                else if (employee)
                {
                    if (checkin.UserId != currentUserId) throw new ArgumentException("Employee chỉ có thể xem checkin của mình");
                }

                return new ResponseModel.CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    CheckinTime = checkin.CheckinTime,
                    CheckoutTime = checkin.CheckoutTime,
                    Name = checkin.Users.Fullname,
                    UserId = checkin.UserId,
                    LogStatus = (int?)checkin.LogStatus,
                    Status = checkin.LogStatus.ToString(),
                    DeviceInfo = checkin.DeviceInfo ?? "",
                    CheckinIP = checkin.CheckinIP,
                    CheckoutIP = checkin.CheckoutIP,
                    TotalTime = checkin.TotalTime,
                    Note = checkin.Note
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<CheckinResultDto> CheckinAsync(Guid? userId, string DeviceInfo, string ip, string Note, Guid currentUserId, IList<string> roles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isAdmin = roles.Contains("Administrator");
                var isManager = roles.Contains("Manager");
                var isEmployee = roles.Contains("Employee");
                var isSystemAdmin = roles.Contains("SystemAdmin");

                Guid targetUserId = userId ?? currentUserId;

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                var targetUser = await _userRepository.GetActiveUserIdAsync(targetUserId);

                if (targetUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng");

                if (string.IsNullOrWhiteSpace(ip))
                    throw new ArgumentException("Không tìm thấy IP");

                if (targetUserId == currentUserId)
                {
                    if (string.IsNullOrWhiteSpace(DeviceInfo))
                        throw new ArgumentException("Không tìm thấy thiết bị");
                }
                else
                {
                    if ((isAdmin || isManager) && string.IsNullOrWhiteSpace(Note))
                        throw new ArgumentException("Khi checkin hộ, Admin hoặc Manager phải nhập lý do.");

                    if (isEmployee)
                        throw new ArgumentException("Employee không thể checkin cho user khác");
                }

                if (!isSystemAdmin && currentUser.CompanyId == null)
                    throw new ArgumentException("Người dùng chưa có công ty.");

                if (isAdmin)
                {
                    if (currentUser.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ System Admin để cập nhật công ty");

                    if (targetUser.CompanyId != currentUser.CompanyId)
                        throw new ArgumentException("Admin chỉ có thể checkin cho user cùng công ty");
                }
                else if (isManager)
                {
                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (targetUser.DepartmentId != currentUser.DepartmentId)
                        throw new ArgumentException("Manager chỉ có thể checkin cho user cùng phòng ban");
                }

                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

                var startOfDay = now.Date;
                var endOfDay = now.Date.AddDays(1).AddTicks(-1);

                if (isEmployee)
                {
                    var otherCheckinToday = await _context.Checkins.Where(c => c.CheckinTime >= startOfDay && c.CheckinTime <= endOfDay && c.UserId != currentUserId && !c.IsDeleted && c.DeviceInfo == DeviceInfo).FirstOrDefaultAsync();
                    if (otherCheckinToday != null)
                        throw new ArgumentException("Thiết bị này đã được dùng để checkin cho người khác hôm nay. Nhân viên không được phép sử dụng chung thiết bị để checkin.");
                }

                string finalDeviceInfo = (targetUserId == currentUserId) ? (DeviceInfo ?? "") : null;

                var (_, _, currentTime, schedule, isHoliday, isSunday) = await GetTimeAndScheduleInfoAsync((Guid)currentUser.CompanyId);

                Enums.LogStatus logStatus;

                if (isHoliday || isSunday)
                {
                    if (currentTime <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime))
                    {
                        logStatus = Enums.LogStatus.OnHoliday;
                    }
                    else
                    {
                        logStatus = Enums.LogStatus.OnHolidayLate;
                    }
                }
                else
                {
                    if (currentTime < schedule.StartTimeMorning ||
                        currentTime > schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime) ||
                        (currentTime > schedule.EndTimeMorning && currentTime < schedule.StartTimeAfternoon))
                    {
                        throw new ArgumentException("Hiện tại không trong khung giờ cho phép để check-in");
                    }

                    if (currentTime <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime))
                    {
                        logStatus = Enums.LogStatus.OnTime;
                    }
                    else
                    {
                        logStatus = Enums.LogStatus.Late;
                    }
                }

                var existingCheckin = await _context.Checkins.FirstOrDefaultAsync(c => c.UserId == targetUserId && c.CheckinTime >= startOfDay && c.CheckinTime <= endOfDay && !c.IsDeleted);

                if (existingCheckin != null)
                    throw new ArgumentException("Đã check-in hôm nay");

                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    CheckinTime = now, 
                    CheckoutTime = DateTime.MinValue,
                    LogStatus = logStatus,
                    DeviceInfo = DeviceInfo ?? "",
                    CheckinIP = ip,
                    CheckoutIP = null,
                    TotalTime = 0,
                    Note = Note ?? string.Empty,
                };

                await _checkinRepository.CreateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    UserId = targetUserId,
                    Name = targetUser.Fullname,
                    CheckinTime = checkin.CheckinTime,
                    CheckoutTime = checkin.CheckoutTime,
                    Status = checkin.LogStatus.ToString(),
                    LogStatus = (int?)checkin.LogStatus,
                    DeviceInfo = DeviceInfo,
                    CheckinIP = checkin.CheckinIP,
                    CheckoutIP = checkin.CheckoutIP ?? null,
                    TotalTime = checkin.TotalTime,
                    Note = Note
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when checking in");
                throw;
            }
        }
        public async Task<CheckinResultDto> CheckoutAsync(Guid? userId, string DeviceInfo, string ip, string Note, Guid currentUserId, IList<string> roles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isAdmin = roles.Contains("Administrator");
                var isManager = roles.Contains("Manager");
                var isEmployee = roles.Contains("Employee");

                Guid targetUserId = userId ?? currentUserId;

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                var targetUser = await _userRepository.GetActiveUserIdAsync(targetUserId);

                if (currentUserId == targetUser.UserId && string.IsNullOrWhiteSpace(DeviceInfo))
                    throw new ArgumentException("Không tìm thấy thiết bị");
                else if (string.IsNullOrWhiteSpace(ip))
                    throw new ArgumentException("Không tìm thấy ip");

                if (isAdmin && currentUser.CompanyId == null)
                    throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ System Admin để cập nhật công ty");
                else if (isManager && currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");
                
                if (targetUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng");

                if (isAdmin && targetUser.CompanyId != currentUser.CompanyId)
                    throw new ArgumentException("Admin chỉ có thể checkout cho user cùng công ty");

                else if (isManager && targetUser.DepartmentId != currentUser.DepartmentId)
                    throw new ArgumentException("Manager chỉ có thể checkout cho user cùng phòng ban");

                else if (isEmployee && targetUserId != currentUserId)
                    throw new ArgumentException("Employee không thể checkout cho user khác");

                var (_, vnTime, currentTime, schedule, isHoliday, isSunday) = await GetTimeAndScheduleInfoAsync((Guid)currentUser.CompanyId);

                var startOfDay = vnTime.Date;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                var checkin = await _context.Checkins
                    .FirstOrDefaultAsync(c => c.UserId == targetUserId && c.CheckinTime >= startOfDay && c.CheckinTime <= endOfDay && !c.IsDeleted);

                if ((isAdmin || isManager) && string.IsNullOrWhiteSpace(Note) && string.IsNullOrWhiteSpace(checkin.Note) && targetUser.UserId != currentUserId)
                    throw new ArgumentException("Khi checkout hộ, Admin hoặc Manager phải nhập lý do.");
                if (!string.IsNullOrWhiteSpace(Note))
                {
                    checkin.Note = Note;
                }
                if (checkin == null)
                    throw new ArgumentException("Không tìm thấy bản ghi checkin hôm nay");

                if(DeviceInfo != checkin.DeviceInfo && checkin.DeviceInfo != null)
                    throw new ArgumentException("Thiết bị hiện tại không khớp với thiết bị lúc checkin");

                if (checkin.CheckoutTime != DateTime.MinValue)
                    throw new ArgumentException("Đã checkout rồi");

                checkin.CheckoutTime = vnTime; // lưu giờ VN
                double overtimeHours = 0;

                var workEndTime = schedule.EndTimeAfternoon;

                if (isHoliday || isSunday)
                {
                    // Ngày nghỉ / Chủ nhật
                    if (checkin.LogStatus == Enums.LogStatus.OnHoliday)
                    {
                        if (currentTime > workEndTime.AddMinutes(schedule.LogAllowtime))
                        {
                            checkin.LogStatus = Enums.LogStatus.OnHolidayOvertime;
                            overtimeHours = (currentTime - workEndTime).TotalHours;
                        }
                        else if (currentTime < workEndTime)
                        {
                            checkin.LogStatus = Enums.LogStatus.OnHolidayLeaveEarly;
                        }
                        else
                        {
                            checkin.LogStatus = Enums.LogStatus.OnHoliday;
                        }
                    }
                    else if (checkin.LogStatus == Enums.LogStatus.OnHolidayLate)
                    {
                        if (currentTime > workEndTime.AddMinutes(schedule.LogAllowtime))
                        {
                            checkin.LogStatus = Enums.LogStatus.OnHolidayLateAndOvertime;
                            overtimeHours = (currentTime - workEndTime).TotalHours;
                        }
                        else if (currentTime < workEndTime)
                        {
                            checkin.LogStatus = Enums.LogStatus.OnHolidayLateAndLeaveEarly;
                        }
                        else
                        {
                            checkin.LogStatus = Enums.LogStatus.OnHolidayLate;
                        }
                    }
                }
                else
                {
                    // Ngày thường
                    if (checkin.LogStatus == Enums.LogStatus.OnTime)
                    {
                        if (currentTime > workEndTime.AddMinutes(schedule.LogAllowtime))
                        {
                            checkin.LogStatus = Enums.LogStatus.Overtime;
                            overtimeHours = (currentTime - workEndTime).TotalHours;
                        }
                        else if (currentTime < workEndTime)
                        {
                            checkin.LogStatus = Enums.LogStatus.LeaveEarly;
                        }
                        else
                        {
                            checkin.LogStatus = Enums.LogStatus.OnTime;
                        }
                    }
                    else if (checkin.LogStatus == Enums.LogStatus.Late)
                    {
                        if (currentTime > workEndTime.AddMinutes(schedule.LogAllowtime))
                        {
                            checkin.LogStatus = Enums.LogStatus.LateAndOvertime;
                            overtimeHours = (currentTime - workEndTime).TotalHours;
                        }
                        else if (currentTime < workEndTime)
                        {
                            checkin.LogStatus = Enums.LogStatus.LateAndLeaveEarly;
                        }
                        else
                        {
                            checkin.LogStatus = Enums.LogStatus.Late;
                        }
                    }
                }

                var totalWorkedHours = (checkin.CheckoutTime - checkin.CheckinTime).TotalHours;
                var lunchBreak = (schedule.StartTimeAfternoon - schedule.EndTimeMorning).TotalHours;
                double normalWorkedHours;

                if (checkin.CheckinTime.TimeOfDay < schedule.EndTimeMorning.ToTimeSpan()
                    && checkin.CheckoutTime.TimeOfDay > schedule.StartTimeAfternoon.ToTimeSpan())
                {
                    normalWorkedHours = Math.Floor(totalWorkedHours - lunchBreak);
                }
                else
                {
                    normalWorkedHours = Math.Floor(totalWorkedHours);
                }

                if (normalWorkedHours < 0) normalWorkedHours = 0;
                checkin.TotalTime = normalWorkedHours;
                checkin.CheckoutIP = ip;

                await _checkinRepository.UpdateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    UserId = targetUserId,
                    Name = targetUser.Fullname,
                    CheckinTime = checkin.CheckinTime,
                    CheckoutTime = checkin.CheckoutTime,
                    LogStatus = (int?)checkin.LogStatus,
                    Status = checkin.LogStatus.ToString(),
                    DeviceInfo = DeviceInfo ?? "",
                    CheckinIP = checkin.CheckinIP,
                    CheckoutIP = checkin.CheckoutIP,
                    TotalTime = checkin.TotalTime,
                    Note = checkin.Note
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when checking out");
                throw;
            }
        }
        private async Task<(DateTime nowUtc, DateTime vnTime, TimeOnly currentTime, ScheduleTime schedule, bool isHoliday, bool isSunday)> GetTimeAndScheduleInfoAsync(Guid companyId)
        {
            var nowUtc = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);
            var currentTime = TimeOnly.FromDateTime(vnTime);

            var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync(s => s.CompanyId == companyId);
            if (schedule == null) throw new Exception("Không tìm thấy cấu hình giờ làm việc cho công ty");

            var isHoliday = await _holidayRepository.IsHolidayAsync(vnTime, companyId);
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

                if (dto.CheckinTime.HasValue)
                    existing.CheckinTime = dto.CheckinTime.Value;
                if (dto.CheckoutTime.HasValue)
                    existing.CheckoutTime = dto.CheckoutTime.Value;
                if(!string.IsNullOrWhiteSpace(dto.UpdateNote))
                    existing.Note = dto.UpdateNote;

                var employee = await _userRepository.GetActiveUserIdAsync(existing.UserId);
                //if (employee == null)
                //    throw new ArgumentException("Không tìm thấy người dùng");

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);

                if(currentUserRoles.Contains("Administrator"))
                {
                    if (currentUser.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ System Admin để cập nhật công ty");

                    if (currentUser.CompanyId != employee.CompanyId)
                        throw new ArgumentException("Admin chỉ có thể cập nhật checkin cho user cùng công ty");
                }
                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new ArgumentException("Manager chỉ có thể cập nhật checkin cho user cùng phòng ban");
                }

                var (_, vnTime, currentTime, schedule, isHoliday, isSunday) = await GetTimeAndScheduleInfoAsync((Guid)currentUser.CompanyId);

                if(dto.CheckoutTime < dto.CheckinTime)
                    throw new ArgumentException("Thời gian checkout không thể trước thời gian checkin");

                Enums.LogStatus logStatus;

                if (existing.CheckoutTime == DateTime.MinValue)
                {
                    if (isHoliday || isSunday)
                    {
                        logStatus = currentTime <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime) ? Enums.LogStatus.OnHoliday : Enums.LogStatus.OnHolidayLate;
                    }
                    else
                    {
                        logStatus = existing.CheckinTime.TimeOfDay <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime).ToTimeSpan() ? Enums.LogStatus.OnTime : Enums.LogStatus.Late;
                    }
                    existing.LogStatus = logStatus;
                    existing.TotalTime = 0;
                }
                else
                {
                    double lunchBreakHours = (schedule.StartTimeAfternoon - schedule.EndTimeMorning).TotalHours;
                    var checkinTime = existing.CheckinTime;
                    var checkoutTime = existing.CheckoutTime;
                    var totalWorkedHours = (checkoutTime - checkinTime).TotalHours;

                    if (checkinTime.TimeOfDay < schedule.EndTimeMorning.ToTimeSpan() && checkoutTime.TimeOfDay > schedule.StartTimeAfternoon.ToTimeSpan())
                        totalWorkedHours -= lunchBreakHours;

                    if (totalWorkedHours < 0) totalWorkedHours = 0;

                    existing.TotalTime = totalWorkedHours;

                    bool isLate = checkinTime.TimeOfDay > schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime).ToTimeSpan();
                    bool leaveEarly = checkoutTime.TimeOfDay < schedule.EndTimeAfternoon.ToTimeSpan();
                    bool isOvertime = checkoutTime.TimeOfDay > schedule.EndTimeAfternoon.ToTimeSpan();

                    if (isHoliday || isSunday)
                    {
                        if (isLate && isOvertime)
                            logStatus = Enums.LogStatus.OnHolidayLateAndOvertime;
                        else if (isLate && leaveEarly)
                            logStatus = Enums.LogStatus.OnHolidayLateAndLeaveEarly;
                        else if (isLate)
                            logStatus = Enums.LogStatus.OnHolidayLate;
                        else if (leaveEarly)
                            logStatus = Enums.LogStatus.OnHolidayLeaveEarly;
                        else if (isOvertime)
                            logStatus = Enums.LogStatus.OnHolidayOvertime;
                        else
                            logStatus = Enums.LogStatus.OnHoliday;
                    }
                    else
                    {
                        if (isLate && isOvertime)
                            logStatus = Enums.LogStatus.LateAndOvertime;
                        else if (isLate && leaveEarly)
                            logStatus = Enums.LogStatus.LateAndLeaveEarly;
                        else if (isLate)
                            logStatus = Enums.LogStatus.Late;
                        else if (leaveEarly)
                            logStatus = Enums.LogStatus.LeaveEarly;
                        else if (isOvertime)
                            logStatus = Enums.LogStatus.Overtime;
                        else
                            logStatus = Enums.LogStatus.OnTime;
                    }

                    existing.LogStatus = logStatus;
                }


                await _checkinRepository.UpdateAsync(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CheckinResultDto
                {
                    CheckinId = existing.Id,
                    UserId = employee.UserId,
                    Name = employee.Fullname,
                    CheckinTime = existing.CheckinTime,
                    CheckoutTime = existing.CheckoutTime,
                    LogStatus = (int?)existing.LogStatus,
                    Status = existing.LogStatus.ToString(),
                    DeviceInfo = existing.DeviceInfo ?? "",
                    CheckinIP = existing.CheckinIP,
                    CheckoutIP = existing.CheckoutIP,
                    TotalTime = existing.TotalTime,
                    Note = existing.Note
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when updating checkin");
                throw;
            }
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
                    if (currentUser.CompanyId == null) throw new ArgumentException("Manager chưa có công ty. Vui lòng liên hệ system Admin để cập nhật công ty");

                    if (currentUser.CompanyId != employee.CompanyId) throw new UnauthorizedAccessException("Admin chỉ có thể xóa checkin của user cùng công ty");
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
        public async Task<PagedResult<UserWithCheckinsDto>> GetUsersWithCheckinsAsync(string? Name, Guid? companyId, Guid? departmentId, Guid? positionId, int? Day, int? Month, int? Year, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var userQuery = _userRepository.GetAll().Where(p => p.Role == RoleType.Manager || p.Role == RoleType.Employee);

                if (currentUserRoles.Contains("Administrator"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Administrator chưa có công ty.");
                    userQuery = userQuery.Where(u => u.CompanyId == currentUser.CompanyId);
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser?.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban.");
                    userQuery = userQuery.Where(u => u.DepartmentId == currentUser.DepartmentId);
                }
                else if (currentUserRoles.Contains("Employee"))
                {
                    userQuery = userQuery.Where(u => u.UserId == currentUserId);
                }

                if (!string.IsNullOrWhiteSpace(Name))
                {
                    Name = Name.ToLower();
                    userQuery = userQuery.Where(u => u.Fullname.ToLower().Contains(Name));
                }

                if (companyId.HasValue)
                    userQuery = userQuery.Where(u => u.CompanyId == companyId);
                if (departmentId.HasValue)
                    userQuery = userQuery.Where(u => u.DepartmentId == departmentId);
                if (positionId.HasValue)
                    userQuery = userQuery.Where(u => u.PositionId == positionId);

                var totalCount = await userQuery.CountAsync();

                var users = await userQuery
                    .OrderBy(u => u.Fullname)
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync();

                var userIds = users.Select(u => u.UserId).ToList();

                var now = DateTime.Now;

                var checkinQuery = _checkinRepository.GetAll().Where(c => userIds.Contains(c.UserId));

                if (Year.HasValue)
                    checkinQuery = checkinQuery.Where(c => c.CheckinTime.Year == Year.Value);
                if (Month.HasValue)
                    checkinQuery = checkinQuery.Where(c => c.CheckinTime.Month == Month.Value);
                if (Day.HasValue)
                    checkinQuery = checkinQuery.Where(c => c.CheckinTime.Day == Day.Value);

                var checkins = await checkinQuery.ToListAsync();

                var result = users.Select(u => new UserWithCheckinsDto
                {
                    UserId = u.UserId,
                    FullName = u.Fullname,
                    //CompanyName = u.Company?.Name ?? string.Empty,
                    //DepartmentName = u.Department?.Name ?? string.Empty,
                    //PositionName = u.Position?.Name ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    ImageUrl = u.ImageUrl,
                    Checkins = checkins
                        .Where(c => c.UserId == u.UserId)
                        .Select(c => new CheckinResultDto
                        {
                            CheckinId = c.Id,
                            Name = c.Users.Fullname,
                            CheckinTime = c.CheckinTime,
                            CheckoutTime = c.CheckoutTime,
                            LogStatus = (int?)c.LogStatus ?? 0,
                            Status = c.LogStatus.ToString(),
                            DeviceInfo = c.DeviceInfo ?? "",
                            CheckinIP = c.CheckinIP,
                            CheckoutIP = c.CheckoutIP,
                            TotalTime = c.TotalTime,
                            Note = c.Note
                        }).ToList()
                }).ToList();

                return new PagedResult<UserWithCheckinsDto>
                {
                    Items = result,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users with checkins: {Message}", ex.Message);
                throw;
            }
        }
    }
}
