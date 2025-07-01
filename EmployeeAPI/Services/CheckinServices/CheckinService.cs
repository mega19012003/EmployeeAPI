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
        
        public async Task<PagedResult<ResponseModel.CheckinResultDto>> GetAllAsync(string? Name, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _checkinRepository.GetAll();

                if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);

                    if (currentUser.DepartmentId == null) throw new ArgumentException("Manager does not belong to any department");

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
                    .Select(c => new ResponseModel.CheckinResultDto
                    {
                        CheckinId = c.Id,
                        CheckinMorning = c.CheckinMorning,
                        CheckinMorningStatus = c.CheckinMorningStatus.ToString(),
                        CheckoutMorning = c.CheckoutMorning,
                        CheckoutMorningStatus = c.CheckoutMorningStatus.ToString(),

                        CheckinAfternoon = c.CheckinAfternoon,
                        CheckinAfternoonStatus = c.CheckinAfternoonStatus.ToString(),
                        CheckoutAfternoon = c.CheckoutAfternoon,
                        CheckoutAfternoonStatus = c.CheckoutAfternoonStatus.ToString(),

                        Name = c.Users.Fullname,
                        //SalaryPerDay = c.SalaryPerDay,
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
                var c = await _checkinRepository.GetByIdAsync(id);
                if (c == null) throw new ArgumentException("Checkin information not found");

                var manager = currentUserRoles.Contains("Manager");
                var employee = currentUserRoles.Contains("Employee");

                if (manager)
                {
                    var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                    if (currentUser.DepartmentId == null) throw new ArgumentException("Manager does not belong to any department");
                    if (c.Users.DepartmentId != currentUser.DepartmentId) throw new UnauthorizedAccessException("Manager cannot access checkin from other department");
                }
                else if (employee)
                {
                    if (c.UserId != currentUserId) throw new UnauthorizedAccessException("Employee can only access their own checkin");
                }

                return new ResponseModel.CheckinResultDto
                {
                    CheckinId = c.Id,
                    CheckinMorning = c.CheckinMorning,
                    CheckinMorningStatus = c.CheckinMorningStatus.ToString(),
                    CheckoutMorning = c.CheckoutMorning,
                    CheckoutMorningStatus = c.CheckinAfternoonStatus.ToString(),

                    CheckinAfternoon = c.CheckinAfternoon,
                    CheckinAfternoonStatus = c.CheckoutAfternoonStatus.ToString(),
                    CheckoutAfternoon = c.CheckoutAfternoon,
                    CheckoutAfternoonStatus = c.CheckoutAfternoonStatus.ToString(),

                    Name = c.Users.Fullname,
                    //SalaryPerDay = c.SalaryPerDay,
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

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                var targetUser = await _userRepository.GetByIdAsync(targetUserId);
                if (targetUser == null)
                    throw new ArgumentException("User not found");

                if (isManager && targetUser.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager can only check-in for users in the same department");

                if (isEmployee && targetUserId != currentUserId)
                    throw new UnauthorizedAccessException("Employee cannot check-in for others");

                var (nowUtc, vnTime, currentTime, schedule, isHoliday, isSunday) = await GetTimeAndScheduleInfoAsync();
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var startOfDay = vnTime.Date;
                var endOfDay = vnTime.Date.AddDays(1).AddTicks(-1);
                var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDay, vnTimeZone);
                var endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDay, vnTimeZone);

                var isMorning = currentTime < schedule.EndTimeMorning.AddMinutes(schedule.LogAllowtime);
                var isAfternoon = currentTime >= schedule.StartTimeAfternoon && currentTime <= schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime);

                if (!isMorning && !isAfternoon)
                    throw new ArgumentException("Hiện tại không trong khung giờ để check-in");

                var existingCheckin = await _context.Checkins.FirstOrDefaultAsync(c => c.UserId == targetUserId &&
                    ((c.CheckinMorning >= startOfDayUtc && c.CheckinMorning <= endOfDayUtc) ||
                     (c.CheckinAfternoon >= startOfDayUtc && c.CheckinAfternoon <= endOfDayUtc)));

                if (existingCheckin != null)
                {
                    if ((isMorning && existingCheckin.CheckinMorningStatus != Enums.LogStatus.None) ||
                        (isAfternoon && existingCheckin.CheckinAfternoonStatus != Enums.LogStatus.None))
                    {
                        throw new ArgumentException("Đã check-in trong khung giờ này hôm nay");
                    }
                }


                var checkin = existingCheckin ?? new Checkin
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    CheckinMorning = DateTime.MinValue,
                    CheckoutMorning = DateTime.MinValue,
                    CheckinAfternoon = DateTime.MinValue,
                    CheckoutAfternoon = DateTime.MinValue,
                    CheckinMorningStatus = Enums.LogStatus.None,
                    CheckoutMorningStatus = Enums.LogStatus.None,
                    CheckinAfternoonStatus = Enums.LogStatus.None,
                    CheckoutAfternoonStatus = Enums.LogStatus.None,
                };

                if (isMorning)
                {
                    checkin.CheckinMorning = nowUtc;

                    if (currentTime <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime))
                    {
                        checkin.CheckinMorningStatus = (isHoliday || isSunday)
                            ? Enums.LogStatus.OnHoliday
                            : Enums.LogStatus.OnTime;
                    }
                    else if (currentTime <= schedule.StartTimeMorning.AddMinutes(schedule.LogAllowtime + schedule.LateThresholdMinutes))
                    {
                        checkin.CheckinMorningStatus = (isHoliday || isSunday)
                            ? Enums.LogStatus.LateOnHoliday
                            : Enums.LogStatus.Late;
                    }
                    else
                    {
                        checkin.CheckinMorningStatus = Enums.LogStatus.Absent;
                        checkin.CheckoutMorningStatus = Enums.LogStatus.Absent;
                    }
                }
                else if (isAfternoon)
                {
                    checkin.CheckinAfternoon = nowUtc;

                    if (currentTime <= schedule.StartTimeAfternoon.AddMinutes(schedule.LogAllowtime))
                    {
                        checkin.CheckinAfternoonStatus = (isHoliday || isSunday)
                            ? Enums.LogStatus.OnHoliday
                            : Enums.LogStatus.OnTime;
                    }
                    else if (currentTime <= schedule.StartTimeAfternoon.AddMinutes(schedule.LogAllowtime + schedule.LateThresholdMinutes))
                    {
                        checkin.CheckinAfternoonStatus = (isHoliday || isSunday)
                            ? Enums.LogStatus.LateOnHoliday
                            : Enums.LogStatus.Late;
                    }
                    else
                    {
                        checkin.CheckinAfternoonStatus = Enums.LogStatus.Absent;
                    }

                    if (existingCheckin == null)
                    {
                        checkin.CheckinMorning = nowUtc;
                        checkin.CheckinMorningStatus = Enums.LogStatus.Absent;
                        checkin.CheckoutMorningStatus = Enums.LogStatus.Absent;
                    }
                }

                checkin.SalaryPerDay = await CalculateSalaryPerDayAsync(targetUser.UserId, checkin.CheckinMorningStatus, checkin.CheckoutMorningStatus, checkin.CheckinAfternoonStatus, checkin.CheckoutAfternoonStatus, 0);

                if (existingCheckin == null)
                {
                    await _checkinRepository.CreateAsync(checkin);
                }
                else
                {
                    await _checkinRepository.UpdateAsync(checkin);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    Name = targetUser.Fullname,
                    CheckinMorning = checkin.CheckinMorning,
                    CheckoutMorning = checkin.CheckoutMorning,
                    CheckinAfternoon = checkin.CheckinAfternoon,
                    CheckoutAfternoon = checkin.CheckoutAfternoon,
                    CheckinMorningStatus = checkin.CheckinMorningStatus.ToString(),
                    CheckoutMorningStatus = checkin.CheckoutMorningStatus.ToString(),
                    CheckinAfternoonStatus = checkin.CheckinAfternoonStatus.ToString(),
                    CheckoutAfternoonStatus = checkin.CheckoutAfternoonStatus.ToString(),
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

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                var targetUser = await _userRepository.GetByIdAsync(targetUserId);
                if (targetUser == null)
                    throw new ArgumentException("User not found");

                if (isManager && targetUser.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager can only check-out for users in the same department");

                if (isEmployee && targetUserId != currentUserId)
                    throw new UnauthorizedAccessException("Employee cannot check-out for others");

                var (nowUtc, vnTime, currentTime, schedule, isHoliday, isSunday) = await GetTimeAndScheduleInfoAsync();
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(vnTime.Date, vnTimeZone);
                var endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(vnTime.Date.AddDays(1).AddTicks(-1), vnTimeZone);

                var checkin = await _context.Checkins.FirstOrDefaultAsync(c => c.UserId == targetUserId &&
                    ((c.CheckinMorning >= startOfDayUtc && c.CheckinMorning <= endOfDayUtc) ||
                     (c.CheckinAfternoon >= startOfDayUtc && c.CheckinAfternoon <= endOfDayUtc)));

                if (checkin == null)
                    throw new ArgumentException("Check-in record not found for today");

                bool isCheckoutMorning = currentTime >= schedule.StartTimeMorning && currentTime <= schedule.EndTimeMorning.AddMinutes(schedule.LogAllowtime);
                bool isCheckoutAfternoonRange = currentTime >= schedule.StartTimeAfternoon && currentTime <= schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime);

                var alreadyCheckedOut = (isCheckoutMorning && checkin.CheckoutMorningStatus != Enums.LogStatus.None) ||
                                         (isCheckoutAfternoonRange && checkin.CheckoutAfternoonStatus != Enums.LogStatus.None);

                if (alreadyCheckedOut)
                    throw new ArgumentException("Already checked out");

                if (isCheckoutMorning)
                {
                    checkin.CheckoutMorning = nowUtc;
                    var workedDuration = checkin.CheckoutMorning - checkin.CheckinMorning;
                    var totalDuration = schedule.EndTimeMorning - schedule.StartTimeMorning;
                    var threshold1 = totalDuration.TotalMinutes * 0.75; // làm dc 3/4 giờ
                    var threshold2 = totalDuration.TotalMinutes * 0.5; // làm dc 1/2 giờ

                    //_logger.LogInformation("ACtual time work: {time}", workedDuration);
                    //_logger.LogInformation("Work schedule: {time}", totalDuration);
                    //_logger.LogInformation("threshold: {time}", threshold);

                    if (workedDuration.TotalMinutes < threshold2)
                    {
                        if (checkin.CheckinMorningStatus == Enums.LogStatus.OnTime || checkin.CheckinMorningStatus == Enums.LogStatus.OnHoliday)
                        {
                            checkin.CheckinMorningStatus = Enums.LogStatus.LeaveEarly;
                        }
                        checkin.CheckoutMorningStatus = Enums.LogStatus.Absent;
                        checkin.CheckinAfternoonStatus = Enums.LogStatus.Absent;
                        checkin.CheckoutAfternoonStatus = Enums.LogStatus.Absent;
                    }
                    else if (workedDuration.TotalMinutes < threshold1)
                    {
                        checkin.CheckoutMorningStatus = Enums.LogStatus.Absent;
                        checkin.CheckinAfternoonStatus = Enums.LogStatus.Absent;
                        checkin.CheckoutAfternoonStatus = Enums.LogStatus.Absent;
                    }
                    else
                    {
                        checkin.CheckoutMorningStatus = Enums.LogStatus.LeaveEarly;
                        checkin.CheckinAfternoonStatus = Enums.LogStatus.Absent;
                        checkin.CheckoutAfternoonStatus = Enums.LogStatus.Absent;
                    }
                }
                else if (isCheckoutAfternoonRange)
                {
                    checkin.CheckoutAfternoon = nowUtc;

                    if (checkin.CheckinAfternoon == DateTime.MinValue)
                    {
                        // Set mặc định là giờ bắt đầu ca chiều
                        var checkinAfternoonTime = vnTime.Date.Add(schedule.StartTimeAfternoon.ToTimeSpan());
                        checkin.CheckinAfternoon = TimeZoneInfo.ConvertTimeToUtc(checkinAfternoonTime, vnTimeZone);
                    }

                    var workedDuration = checkin.CheckoutAfternoon - checkin.CheckinAfternoon;
                    var totalDuration = schedule.EndTimeAfternoon - schedule.StartTimeAfternoon;
                    var threshold1 = totalDuration.TotalMinutes * 0.75; // làm dc 3/4 giờ
                    var threshold2 = totalDuration.TotalMinutes * 0.5; // làm dc 1/2 giờ
                    //var threshold3 = totalDuration.TotalMinutes * 0.25; // làm dc 1/4 giờ
                    //_logger.LogInformation("Actual worked time: {time}", workedDuration);
                    //_logger.LogInformation("Work schedule: {time}", totalDuration);
                    //_logger.LogInformation("Threshold: {time}", threshold);

                    if (workedDuration.TotalMinutes < threshold2)
                    {
                        // Làm < 50% thời gian
                        checkin.CheckinAfternoonStatus = Enums.LogStatus.Absent;
                        checkin.CheckoutAfternoonStatus = Enums.LogStatus.Absent;
                    }
                    else if (workedDuration.TotalMinutes < threshold1)
                    {
                        // Làm 50% đến <75% 
                        checkin.CheckinAfternoonStatus = Enums.LogStatus.LeaveEarly;
                        checkin.CheckoutAfternoonStatus = Enums.LogStatus.Absent;
                    }
                    else if (currentTime <= schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime))
                    {
                        // Làm >75% 
                        checkin.CheckinAfternoonStatus = Enums.LogStatus.OnTime;
                        checkin.CheckoutAfternoonStatus = Enums.LogStatus.LeaveEarly;
                    }
                    else
                    {
                        checkin.CheckoutAfternoonStatus = Enums.LogStatus.Overtime;
                    }

                    if (checkin.CheckinMorningStatus == Enums.LogStatus.Absent)
                    {
                        checkin.CheckoutMorningStatus = Enums.LogStatus.Absent;
                    }
                    else if (checkin.CheckoutMorningStatus == Enums.LogStatus.None)
                    {
                        checkin.CheckoutMorningStatus = isHoliday || isSunday ? Enums.LogStatus.OnHoliday : Enums.LogStatus.OnTime;
                    }

                    if (checkin.CheckinAfternoonStatus == Enums.LogStatus.None)
                    {
                        checkin.CheckinAfternoonStatus = isHoliday || isSunday ? Enums.LogStatus.OnHoliday : Enums.LogStatus.OnTime;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Not within valid checkout time range");
                }
                var overtimeDuration = 1;
                var checkouTime = vnTime.Hour;
                var endTime = schedule.EndTimeAfternoon.Hour;
                if (checkouTime > endTime)
                {
                    overtimeDuration = checkouTime - endTime;
                }
                double salaryPerDay = await CalculateSalaryPerDayAsync(targetUser.UserId, checkin.CheckinMorningStatus, checkin.CheckoutMorningStatus, checkin.CheckinAfternoonStatus, checkin.CheckoutAfternoonStatus, overtimeDuration);
                await _checkinRepository.UpdateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinResultDto
                {
                    CheckinId = checkin.Id,
                    Name = targetUser.Fullname,
                    CheckinMorning = checkin.CheckinMorning,
                    CheckoutMorning = checkin.CheckoutMorning,
                    CheckinAfternoon = checkin.CheckinAfternoon,
                    CheckoutAfternoon = checkin.CheckoutAfternoon,
                    CheckinMorningStatus = checkin.CheckinMorningStatus.ToString(),
                    CheckoutMorningStatus = checkin.CheckoutMorningStatus.ToString(),
                    CheckinAfternoonStatus = checkin.CheckinAfternoonStatus.ToString(),
                    CheckoutAfternoonStatus = checkin.CheckoutAfternoonStatus.ToString(),
                    SalaryPerDay = salaryPerDay
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error when checking out");
                throw;
            }
        }
        private async Task<(DateTime nowUtc, DateTime vnTime, TimeOnly currentTime, ScheduleTime schedule, bool isHoliday, bool isSunday)> GetTimeAndScheduleInfoAsync()
        {
            var nowUtc = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);
            var currentTime = TimeOnly.FromDateTime(vnTime);

            var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
            if (schedule == null) throw new Exception("Schedule not found");

            var isHoliday = await _holidayRepository.IsHolidayAsync(nowUtc);
            var isSunday = vnTime.DayOfWeek == DayOfWeek.Sunday;

            return (nowUtc, vnTime, currentTime, schedule, isHoliday, isSunday);
        }

        public async Task<ResponseModel.CheckinResultDto> UpdateAsync(ResponseModel.UpdateCheckinDto dto, Guid currentUserId, IList<string> currentUserRoles)
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

                var (nowUtc, vnTime, currentTime, schedule, isHoliday, isSunday) = await GetTimeAndScheduleInfoAsync();
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(vnTime.Date, vnTimeZone);
                var endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(vnTime.Date.AddDays(1).AddTicks(-1), vnTimeZone);
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

                var currentTimeOnly = TimeOnly.FromDateTime(nowVn);
                var overtimeThreshold = schedule.EndTimeAfternoon.AddMinutes(schedule.LateThresholdMinutes);


                existing.CheckinMorningStatus = dto.CheckinMorningStatus;
                existing.CheckoutMorningStatus = dto.CheckoutMorningStatus;
                existing.CheckinAfternoonStatus = dto.CheckinAfternoonStatus;
                existing.CheckoutAfternoonStatus = dto.CheckoutAfternoonStatus;

                var overtimeDuration = 1;
                var checkouTime = vnTime.Hour;
                var endTime = schedule.EndTimeAfternoon.Hour;
                if (checkouTime > endTime)
                {
                    overtimeDuration = checkouTime - endTime;
                }
                //existing.SalaryPerDay = await CalculateSalaryPerDayAsync(employee, existing.CheckinMorningStatus, existing.CheckoutAfternoonStatus/*, OvertimeDuration*/);
                double salaryPerDay = await CalculateSalaryPerDayAsync(employee.UserId, existing.CheckinMorningStatus, existing.CheckoutMorningStatus, existing.CheckinAfternoonStatus, existing.CheckoutAfternoonStatus, overtimeDuration);
                existing.SalaryPerDay = salaryPerDay;
                await _checkinRepository.UpdateAsync(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CheckinResultDto
                {
                    CheckinId = existing.Id,
                    CheckinMorning = existing.CheckinMorning,
                    CheckinAfternoonStatus = existing.CheckinAfternoonStatus.ToString(),
                    CheckoutMorning = existing.CheckoutMorning,
                    CheckinMorningStatus = existing.CheckinMorningStatus.ToString(),

                    CheckinAfternoon = existing.CheckinAfternoon,
                    CheckoutAfternoonStatus = existing.CheckoutAfternoonStatus.ToString(),
                    CheckoutAfternoon = existing.CheckoutAfternoon,
                    CheckoutMorningStatus = existing.CheckoutMorningStatus.ToString(),

                    Name = employee.Fullname,
                    SalaryPerDay = existing.SalaryPerDay
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<double> CalculateSalaryPerDayAsync(Guid userId, Enums.LogStatus? CheckinMorningStatus, Enums.LogStatus? CheckoutMorningStatus, Enums.LogStatus? CheckinAfternoonStatus, Enums.LogStatus? CheckoutAfternoonStatus, double overtimeDuration)
        {
            var logStatus = await _logStatusConfigRepository.GetAllAsync();
            var user = await _userRepository.GetByIdAsync(userId);
            ScheduleTime schedule;
            schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();


            double checkinMorningMultiply = 0;
            double checkoutMorningMultiply = 0;
            double checkinAfternoonMultiply = 0;
            double checkoutAfternoonMultiply = 0;

            foreach (var item in logStatus)
            {
                if (item.Id == (int)(CheckinMorningStatus ?? Enums.LogStatus.None))
                {
                    checkinMorningMultiply = item.SalaryMultiplier;
                    //_logger.LogInformation("CheckinMorningStatus: {logstatuscs}", CheckinMorningStatus);
                }
                if (item.Id == (int)(CheckoutMorningStatus ?? Enums.LogStatus.None))
                {
                    checkoutMorningMultiply = item.SalaryMultiplier;
                    //_logger.LogInformation("CheckoutMorningStatus: {logstatuscos}", CheckoutMorningStatus);
                }
                if (item.Id == (int)(CheckoutAfternoonStatus ?? Enums.LogStatus.None))
                {
                    checkoutAfternoonMultiply = item.SalaryMultiplier;
                    //_logger.LogInformation("CheckoutAfternoonStatus: {logstatuscc}", CheckoutMorningStatus);
                }
                if (item.Id == (int)(CheckinAfternoonStatus ?? Enums.LogStatus.None))
                {
                    checkinAfternoonMultiply = item.SalaryMultiplier;
                    //_logger.LogInformation("CheckinAfternoonStatus: {logstatuscoc}", CheckinAfternoonStatus);
                }
            }

            double baseSalary = user.BasicSalary;
            double quarterSalary = baseSalary / 4.0;
            double salaryToday = 0;

            if (CheckoutAfternoonStatus == Enums.LogStatus.Overtime)
            {
                salaryToday = (quarterSalary * checkinMorningMultiply) + (quarterSalary * checkoutMorningMultiply) + (quarterSalary * checkinAfternoonMultiply) + (quarterSalary * checkoutAfternoonMultiply * overtimeDuration);
                //_logger.LogInformation("sang 1 {moneys} - {cs}", quarterSalary * checkinMorningMultiply, checkinMorningMultiply);
                //_logger.LogInformation("sang 2 {moneys} - {cos}", quarterSalary * checkoutMorningMultiply, checkoutMorningMultiply);
                //_logger.LogInformation("chieu 1 {moneys} - {cc}", quarterSalary * checkinAfternoonMultiply, checkinAfternoonMultiply);
                //_logger.LogInformation("chieu 2 {moneys} - {coc}", quarterSalary * checkoutAfternoonMultiply, checkoutAfternoonMultiply);
                //_logger.LogInformation("Over TIme: {OT}", overtimeDuration);
            }
            else salaryToday = (quarterSalary * checkinMorningMultiply) + (quarterSalary * checkoutMorningMultiply) + (quarterSalary * checkinAfternoonMultiply) + (quarterSalary * checkoutAfternoonMultiply);

            return salaryToday;
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
                    if (currentUser.DepartmentId == null) throw new ArgumentException("Manager does not belong to any department");

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
                var now = DateTime.Now;
                Year ??= now.Year;


                var user = await _userRepository.GetByIdAsync(staffId.Value);


                if (user == null)
                    throw new ArgumentException("Cannot find user id");

                var query = _context.Checkins.Where(p => !p.IsDeleted && p.UserId == staffId.Value);
                query = query.Where(c => c.CheckinMorning.Year == Year.Value);

                if (Month.HasValue)
                    query = query.Where(c => c.CheckinMorning.Month == Month.Value);

                if (Day.HasValue)
                    query = query.Where(c => c.CheckinMorning.Day == Day.Value);

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
                        CheckinMorningStatus = c.CheckinMorningStatus.ToString(),
                        CheckoutMorningStatus = c.CheckoutMorningStatus.ToString(),
                        CheckinAfternoonStatus = c.CheckinAfternoonStatus.ToString(),
                        CheckoutAfternoonStatus = c.CheckoutAfternoonStatus.ToString(),
                        Name = c.Users.Fullname,
                        SalaryPerDay = c.SalaryPerDay,
                        TimeCheckin = c.CheckinMorning.Date
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
