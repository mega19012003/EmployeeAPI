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

                /*var query = _context.Checkins.Include(c => c.Users).Where(c => !c.IsDeleted);*/
                var query = _checkinRepository.GetAll();

                if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("User not found");

                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");

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
                        /*updateBy = c.updateBy,
                        UpdateAt = c.UpdateAt*/
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
                    /*updateBy = c.updateBy,
                    UpdateAt = c.UpdateAt*/
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<ResponseModel.CheckinDto> CreateAsync(ResponseModel.CreateCheckin dto, Guid currentUserId, IList<string> currentUserRoles)
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
                    targetUserId = dto.userId == Guid.Empty ? currentUserId : dto.userId;
                }
                else if (isManager)
                {
                    targetUserId = dto.userId == Guid.Empty ? currentUserId : dto.userId;

                    var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                    var targetUser = await _userRepository.GetByIdAsync(targetUserId);

                    if (currentUser == null || targetUser == null)
                        throw new ArgumentException("Không tìm thấy người dùng.");

                    if (currentUser.DepartmentId != targetUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager chỉ được check-in cho nhân viên cùng phòng ban.");
                }
                else if (isEmployee)
                {
                    targetUserId = currentUserId; // Bỏ qua dto.userId
                }
                else
                {
                    throw new UnauthorizedAccessException("Vai trò không hợp lệ để thực hiện check-in.");
                }

                var existUser = await _userRepository.GetByIdAsync(targetUserId);
                if (existUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng.");

                // TimeZone VN
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                // Xử lý thời gian
                DateTime utcCheckinDate;
                DateTime vnCheckinDate;

                if (dto.CheckinDate.HasValue)
                {
                    var inputDate = dto.CheckinDate.Value;
                    if (inputDate.Kind == DateTimeKind.Utc)
                    {
                        vnCheckinDate = TimeZoneInfo.ConvertTimeFromUtc(inputDate, vnTimeZone);
                        utcCheckinDate = inputDate;
                    }
                    else if (inputDate.Kind == DateTimeKind.Local)
                    {
                        utcCheckinDate = inputDate.ToUniversalTime();
                        vnCheckinDate = TimeZoneInfo.ConvertTimeFromUtc(utcCheckinDate, vnTimeZone);
                    }
                    else
                    {
                        vnCheckinDate = inputDate;
                        utcCheckinDate = TimeZoneInfo.ConvertTimeToUtc(vnCheckinDate, vnTimeZone);
                    }
                }
                else
                {
                    utcCheckinDate = DateTime.UtcNow;
                    vnCheckinDate = TimeZoneInfo.ConvertTimeFromUtc(utcCheckinDate, vnTimeZone);
                }

                // Kiểm tra đã check-in ngày đó chưa (giờ VN)
                var alreadyCheckedIn = _context.Checkins
                    .Where(c => c.UserId == targetUserId && !c.IsDeleted)
                    .AsEnumerable()
                    .Any(c =>
                        TimeZoneInfo.ConvertTimeFromUtc(c.CheckinDate, vnTimeZone).Date == vnCheckinDate.Date
                    );

                if (alreadyCheckedIn)
                    throw new ArgumentException("Bạn đã check-in ngày này rồi.");

                var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
                if (schedule == null)
                    throw new Exception("Chưa thiết lập giờ làm việc.");

                var lateTime = schedule.StartTime.AddMinutes(schedule.LateThresholdMinutes);
                var currentTimeOnly = TimeOnly.FromDateTime(vnCheckinDate);

                bool isSunday = vnCheckinDate.DayOfWeek == DayOfWeek.Sunday;
                bool isHoliday = await _holidayRepository.IsHolidayAsync(utcCheckinDate);

                CheckinStatus status;
                if (isSunday || isHoliday)
                    status = CheckinStatus.Overtime;
                else if (currentTimeOnly <= lateTime)
                    status = CheckinStatus.OnTime;
                else
                    status = CheckinStatus.Late;

                var checkin = new Checkin
                {
                    Id = Guid.NewGuid(),
                    CheckinDate = utcCheckinDate,
                    CheckinStatus = status,
                    UserId = targetUserId,
                    /*updateBy = string.Empty,
                    UpdateAt = DateTime.MinValue,*/
                };

                await _checkinRepository.CreateAsync(checkin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CheckinDto
                {
                    CheckinId = checkin.Id,
                    CheckinDate = checkin.CheckinDate,
                    CheckinStatus = checkin.CheckinStatus,
                    Checkin = checkin.CheckinStatus.ToString(),
                    CheckoutDate = checkin.CheckoutDate,
                    CheckoutStatus = checkin.CheckoutStatus,
                    Checkout = checkin.CheckoutStatus.ToString(),
                    userId = checkin.UserId,
                    Name = existUser.Fullname,
                    /*UpdateAt = checkin.UpdateAt,
                    updateBy = checkin.updateBy,*/
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo check-in");
                throw;
            }
        }
        //public async Task<ResponseModel.CheckinDto> CreateAsync(ResponseModel.CreateCheckin dto)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var existUsers = await _userRepository.GetByIdAsync(dto.userId);
        //        if (existUsers == null)
        //            throw new ArgumentException("Cannot find Users");

        //        // 2. Lấy múi giờ VN
        //        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        //        // 3. Xử lý thời gian check-in:
        //        // Nếu client gửi thời gian rồi (giờ VN) thì convert sang UTC để lưu DB
        //        // Nếu không thì lấy giờ hiện tại UTC để lưu
        //        DateTime utcCheckinDate;
        //        DateTime vnCheckinDate;

        //        if (dto.CheckinDate.HasValue)
        //        {
        //            var inputDate = dto.CheckinDate.Value;

        //            if (inputDate.Kind == DateTimeKind.Utc)
        //            {
        //                // Nếu client gửi giờ UTC, convert sang giờ VN để xử lý
        //                vnCheckinDate = TimeZoneInfo.ConvertTimeFromUtc(inputDate, vnTimeZone);
        //                utcCheckinDate = inputDate; // Giữ nguyên UTC để lưu DB
        //            }
        //            else if (inputDate.Kind == DateTimeKind.Local)
        //            {
        //                // Nếu local, convert local sang UTC
        //                utcCheckinDate = inputDate.ToUniversalTime();
        //                vnCheckinDate = TimeZoneInfo.ConvertTimeFromUtc(utcCheckinDate, vnTimeZone);
        //            }
        //            else
        //            {
        //                // Unspecified (không có Kind), giả định đây là giờ VN
        //                vnCheckinDate = inputDate;
        //                utcCheckinDate = TimeZoneInfo.ConvertTimeToUtc(vnCheckinDate, vnTimeZone);
        //            }
        //        }
        //        else
        //        {
        //            utcCheckinDate = DateTime.UtcNow;
        //            vnCheckinDate = TimeZoneInfo.ConvertTimeFromUtc(utcCheckinDate, vnTimeZone);
        //        }

        //        // 4. Kiểm tra đã check-in ngày đó chưa (theo giờ VN)
        //        var alreadyCheckedIn = _context.Checkins
        //            .Where(c => c.UserId == dto.userId)
        //            .AsEnumerable()
        //            .Any(c =>
        //                TimeZoneInfo.ConvertTimeFromUtc(c.CheckinDate, vnTimeZone).Date == vnCheckinDate.Date
        //            );

        //        if (alreadyCheckedIn)
        //            throw new ArgumentException("Bạn đã check-in ngày này rồi.");

        //        // 5. Lấy giờ làm việc trong bảng cấu hình
        //        var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
        //        if (schedule == null)
        //            throw new Exception("Chưa thiết lập giờ làm việc");

        //        // 6. Tính thời gian muộn
        //        var lateTime = schedule.StartTime.AddMinutes(schedule.LateThresholdMinutes);

        //        // Giờ hiện tại (theo VN) dưới dạng TimeOnly để so sánh với giờ bắt đầu
        //        var currentTimeOnly = TimeOnly.FromDateTime(vnCheckinDate);

        //        // 7. Kiểm tra ngày nghỉ lễ hoặc Chủ Nhật
        //        bool isSunday = vnCheckinDate.DayOfWeek == DayOfWeek.Sunday;
        //        bool isHoliday = await _holidayRepository.IsHolidayAsync(utcCheckinDate); // dùng UTC để chuẩn hóa

        //        // 8. Xác định trạng thái check-in
        //        CheckinStatus status;
        //        if (isSunday || isHoliday)
        //        {
        //            status = CheckinStatus.Overtime;
        //        }
        //        else if (currentTimeOnly <= lateTime)
        //        {
        //            status = CheckinStatus.OnTime;
        //        }
        //        else
        //        {
        //            status = CheckinStatus.Late;
        //        }

        //        var checkin = new Checkin
        //        {
        //            Id = Guid.NewGuid(),
        //            CheckinDate = utcCheckinDate,
        //            CheckinStatus = status,
        //            UserId = dto.userId,
        //            updateBy = string.Empty,
        //            UpdateAt = DateTime.MinValue,
        //        };

        //        await _checkinRepository.CreateAsync(checkin);
        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return new ResponseModel.CheckinDto
        //        {
        //            CheckinId = checkin.Id,
        //            CheckinDate = checkin.CheckinDate,
        //            CheckinStatus = checkin.CheckinStatus,
        //            Checkin = checkin.CheckinStatus.ToString(),
        //            CheckoutDate = checkin.CheckoutDate,
        //            CheckoutStatus = checkin.CheckoutStatus,
        //            Checkout = checkin.CheckoutStatus.ToString(),
        //            userId = checkin.UserId,
        //            Name = existUsers.Fullname,
        //            UpdateAt = checkin.UpdateAt,
        //            updateBy = checkin.updateBy,
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Error while creating checkin");
        //        throw;
        //    }
        //}

        //public async Task<ResponseModel.CheckinDto> CheckoutAsync(ResponseModel.CreateCheckout dto)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var existUser = await _userRepository.GetByIdAsync(dto.userId);
        //        if (existUser == null)
        //            throw new ArgumentException("Cannot find User");

        //        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        //        DateTime nowVn;

        //        if (dto.CheckoutDate.HasValue)
        //        {
        //            var dt = dto.CheckoutDate.Value;
        //            // Nếu Kind = Unspecified, giả định là giờ local VN
        //            if (dt.Kind == DateTimeKind.Unspecified)
        //            {
        //                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
        //            }
        //            // Convert giờ local hoặc UTC về giờ VN
        //            nowVn = dt.Kind == DateTimeKind.Utc
        //                ? TimeZoneInfo.ConvertTimeFromUtc(dt, vnTimeZone)
        //                : TimeZoneInfo.ConvertTime(dt, vnTimeZone);
        //        }
        //        else
        //        {
        //            // Lấy giờ hiện tại theo giờ VN
        //            var nowUtc = DateTime.UtcNow;
        //            nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);
        //        }

        //        var checkins = await _context.Checkins
        //            .Where(c => c.UserId == dto.userId && !c.IsDeleted && c.CheckoutDate == default)
        //            .ToListAsync();

        //        // Tìm checkin hôm nay theo giờ VN
        //        var todayCheckin = checkins.FirstOrDefault(c =>
        //        {
        //            // Đảm bảo CheckinDate có Kind phù hợp
        //            var checkinDate = c.CheckinDate;
        //            if (checkinDate.Kind == DateTimeKind.Unspecified)
        //            {
        //                // Giả định giờ trong DB là UTC (hoặc tùy theo bạn lưu thế nào)
        //                checkinDate = DateTime.SpecifyKind(checkinDate, DateTimeKind.Utc);
        //            }

        //            var vnDate = checkinDate.Kind == DateTimeKind.Utc
        //                ? TimeZoneInfo.ConvertTimeFromUtc(checkinDate, vnTimeZone)
        //                : TimeZoneInfo.ConvertTime(checkinDate, vnTimeZone);

        //            return vnDate.Date == nowVn.Date;
        //        });

        //        if (todayCheckin == null)
        //            throw new ArgumentException("Bạn chưa check-in hôm nay hoặc đã checkout rồi.");

        //        // Lấy cấu hình giờ làm việc
        //        var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
        //        if (schedule == null)
        //            throw new Exception("Chưa thiết lập giờ làm việc");

        //        var endWorkTime = schedule.EndTime; 

        //        var currentTimeOnly = TimeOnly.FromDateTime(nowVn);

        //        CheckinStatus newStatus;

        //        if (currentTimeOnly > endWorkTime)
        //        {
        //            newStatus = CheckinStatus.Overtime;
        //        }
        //        else if (currentTimeOnly < endWorkTime)
        //        {
        //            newStatus = CheckinStatus.LeaveEarly;
        //        }
        //        else
        //        {
        //            newStatus = CheckinStatus.OnTime;
        //        }

        //        // Cập nhật giờ checkout, convert giờ VN về UTC trước khi lưu
        //        var checkoutUtc = nowVn.Kind switch
        //        {
        //            DateTimeKind.Utc => nowVn,
        //            DateTimeKind.Local => nowVn.ToUniversalTime(),
        //            DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeToUtc(nowVn, vnTimeZone),
        //            _ => TimeZoneInfo.ConvertTimeToUtc(nowVn, vnTimeZone)
        //        };

        //        todayCheckin.CheckoutDate = checkoutUtc;
        //        todayCheckin.CheckoutStatus = newStatus;

        //        todayCheckin.SalaryPerDay = await CalculateSalaryPerDay.CalculateSalaryPerDayAsync(_context, existUser, todayCheckin.CheckinStatus, todayCheckin.CheckoutStatus);

        //        _context.Checkins.Update(todayCheckin);
        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return new ResponseModel.CheckinDto
        //        {
        //            CheckinId = todayCheckin.Id,
        //            CheckinDate = todayCheckin.CheckinDate,
        //            CheckinStatus = todayCheckin.CheckinStatus,
        //            Checkin = todayCheckin.CheckinStatus.ToString(),
        //            CheckoutDate = todayCheckin.CheckoutDate,
        //            CheckoutStatus = todayCheckin.CheckoutStatus,
        //            Checkout = todayCheckin.CheckoutStatus.ToString(),
        //            userId = todayCheckin.UserId,
        //            Name = existUser.Fullname,
        //            SalaryPerDay = todayCheckin.SalaryPerDay,
        //            updateBy = todayCheckin.updateBy,
        //            UpdateAt = todayCheckin.UpdateAt
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Error while creating checkout");
        //        throw;
        //    }
        //}

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
                    targetUserId = dto.userId == Guid.Empty ? currentUserId : dto.userId;
                }
                else if (isManager)
                {
                    targetUserId = dto.userId == Guid.Empty ? currentUserId : dto.userId;

                    var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                    var targetUser = await _userRepository.GetByIdAsync(targetUserId);

                    if (currentUser == null || targetUser == null)
                        throw new ArgumentException("Không tìm thấy người dùng.");

                    if (currentUser.DepartmentId != targetUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager chỉ được checkout cho nhân viên cùng phòng ban.");
                }
                else if (isEmployee)
                {
                    targetUserId = currentUserId; // Bỏ qua dto.userId
                }
                else
                {
                    throw new UnauthorizedAccessException("Vai trò không hợp lệ để thực hiện checkout.");
                }

                var existUser = await _userRepository.GetByIdAsync(targetUserId);
                if (existUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng.");

                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime nowVn;

                if (dto.CheckoutDate.HasValue)
                {
                    var dt = dto.CheckoutDate.Value;
                    if (dt.Kind == DateTimeKind.Unspecified)
                        dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);

                    nowVn = dt.Kind == DateTimeKind.Utc
                        ? TimeZoneInfo.ConvertTimeFromUtc(dt, vnTimeZone)
                        : TimeZoneInfo.ConvertTime(dt, vnTimeZone);
                }
                else
                {
                    var nowUtc = DateTime.UtcNow;
                    nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTimeZone);
                }

                var checkins = await _context.Checkins
                    .Where(c => c.UserId == targetUserId && !c.IsDeleted && c.CheckoutDate == default)
                    .ToListAsync();

                var todayCheckin = checkins.FirstOrDefault(c =>
                {
                    var checkinDate = c.CheckinDate;
                    if (checkinDate.Kind == DateTimeKind.Unspecified)
                        checkinDate = DateTime.SpecifyKind(checkinDate, DateTimeKind.Utc);

                    var vnDate = checkinDate.Kind == DateTimeKind.Utc
                        ? TimeZoneInfo.ConvertTimeFromUtc(checkinDate, vnTimeZone)
                        : TimeZoneInfo.ConvertTime(checkinDate, vnTimeZone);

                    return vnDate.Date == nowVn.Date;
                });

                if (todayCheckin == null)
                    throw new ArgumentException("Bạn chưa check-in hôm nay hoặc đã checkout rồi.");

                var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
                if (schedule == null)
                    throw new Exception("Chưa thiết lập giờ làm việc");

                var endWorkTime = schedule.EndTime;
                var currentTimeOnly = TimeOnly.FromDateTime(nowVn);

                CheckinStatus newStatus;

                if (currentTimeOnly > endWorkTime)
                    newStatus = CheckinStatus.Overtime;
                else if (currentTimeOnly < endWorkTime)
                    newStatus = CheckinStatus.LeaveEarly;
                else
                    newStatus = CheckinStatus.OnTime;

                var checkoutUtc = nowVn.Kind switch
                {
                    DateTimeKind.Utc => nowVn,
                    DateTimeKind.Local => nowVn.ToUniversalTime(),
                    DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeToUtc(nowVn, vnTimeZone),
                    _ => TimeZoneInfo.ConvertTimeToUtc(nowVn, vnTimeZone)
                };

                todayCheckin.CheckoutDate = checkoutUtc;
                todayCheckin.CheckoutStatus = newStatus;

                todayCheckin.SalaryPerDay = await CalculateSalaryPerDay.CalculateSalaryPerDayAsync(
                    _context,
                    existUser,
                    todayCheckin.CheckinStatus,
                    todayCheckin.CheckoutStatus
                );

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
                    SalaryPerDay = todayCheckin.SalaryPerDay,
                    /*updateBy = todayCheckin.updateBy,
                    UpdateAt = todayCheckin.UpdateAt*/
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi checkout");
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
                    throw new ArgumentException("Cannot find checkin");

                var employee = await _userRepository.GetByIdAsync(existing.UserId);
                if (employee == null)
                    throw new ArgumentException("Cannot find employee for this checkin");

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUserRoles.Contains("Manager"))
                {
                    // Manager: chỉ được update checkin nhân viên trong cùng phòng ban
                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot update checkin made by employeee from other department");

                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");
                }

                existing.CheckinStatus = dto.CheckinStatus;
                existing.CheckoutStatus = dto.CheckoutStatus;
                existing.SalaryPerDay = await CalculateSalaryPerDay.CalculateSalaryPerDayAsync(_context, employee, existing.CheckinStatus, existing.CheckoutStatus);
                /*existing.updateBy = currentUser.Fullname;
                existing.UpdateAt = DateTime.UtcNow;*/

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
                    /*updateBy = existing.updateBy,
                    UpdateAt = existing.UpdateAt*/
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
                _logger.LogInformation("Hôm nay là ngày nghỉ lễ, không đánh dấu Absent.");
                return;
            }

            if (TimeOnly.FromDateTime(vnNow) < endTime)
            {
                _logger.LogInformation("Chưa đến giờ kết thúc ca làm, chưa đánh dấu Absent.");
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
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            var allUsers = await _userRepository.GetAll().ToListAsync();

            var absentUsers = allUsers
                .Where(u => !checkedInUserIds.Contains(u.UserId))
                .ToList();

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

            _logger.LogInformation($"Đã đánh dấu {absentUsers.Count} người dùng vắng mặt ngày {today:dd/MM/yyyy}.");
        }

        public async Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _checkinRepository.GetByIdAsync(id);
                if (existing == null)
                    throw new ArgumentException("Cannot find checkin");

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
                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");

                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot delete checkin from other department");
                }
                else
                {
                    throw new UnauthorizedAccessException("Access Denied");
                }

                /*existing.UpdateAt = DateTime.UtcNow;
                existing.updateBy = currentUser.Fullname;*/

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
                else if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("User not found");

                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");

                    if (staffId == null || staffId == Guid.Empty)
                        throw new ArgumentException("Please input userid");

                    // Kiểm tra user được lấy có tồn tại không
                    var findUser = await _userRepository.GetByIdAsync(staffId.Value);
                    if (findUser == null)
                        throw new ArgumentException("Cannot find user id");

                    if (findUser.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot access checkins from other departments");

                }
                else if (currentUserRoles.Contains("Administrator"))
                {
                    // Admin: bắt buộc phải nhập staffId
                    if (staffId == null || staffId == Guid.Empty)
                        throw new ArgumentException("Please input user id");
                }

                pageIndex ??= 1;
                pageSize ??= 10;
                
                var user = await _userRepository.GetByIdAsync(staffId.Value);
                if (user == null)
                    throw new ArgumentException("Cannot find user id");

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
                        CheckinStatus = c.CheckinStatus,
                        Checkin = c.CheckinStatus.ToString(),
                        CheckoutDate = c.CheckoutDate,
                        CheckoutStatus = c.CheckoutStatus,
                        Checkout = c.CheckoutStatus.ToString(),
                        userId = c.UserId,
                        Name = c.Users.Fullname,
                        SalaryPerDay = c.SalaryPerDay,
                        /*updateBy = c.updateBy,
                        UpdateAt = c.UpdateAt*/
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
