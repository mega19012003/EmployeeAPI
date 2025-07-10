using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.LogStatusConfigs;
using EmployeeAPI.Repositories.Payrolls;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.PayrollServices
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICheckinRepository _checkinRepository;
        private readonly ILogStatusConfigRepository _logStatusConfigRepository;
        private readonly ILogger<PayrollService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        public PayrollService(IPayrollRepository payrollRepository, ILogStatusConfigRepository logStatusConfigRepository, IUserRepository userRepository, ICheckinRepository checkinRepository, ILogger<PayrollService> logger, AppDbContext context)
        {
            _payrollRepository = payrollRepository;

            _userRepository = userRepository;
            _checkinRepository = checkinRepository;
            _logStatusConfigRepository = logStatusConfigRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<PagedResult<ResponseModel.PayrollResultDto>> GetAllPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, int? Day, int? Month, int? Year, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Payrolls
                .Include(p => p.Users)
                .Where(p => !p.IsDeleted);

            if (currentUserRoles.Contains("Manager"))
            {
                var manager = await _context.Users.FindAsync(currentUserId);
                if (manager == null)
                    throw new ArgumentException("Không thể tìm thấy người dùng hiện tại");

                if (manager.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                var departmentId = manager.DepartmentId;

                query = query.Where(p => p.Users.DepartmentId == departmentId);
            }
            if( currentUserRoles.Contains("Employee"))
            {
                var employee = await _context.Users.FindAsync(currentUserId);
                if (employee == null)
                    throw new ArgumentException("Employee not found");
                query = query.Where(p => p.UserId == employee.UserId);
            }

            ///////////////////
            var now = DateTime.Now;
            ////var now = DateTime.Now;
            if (Year == null)
                Year = now.Year;
            //else if (Year == 0)
            //    Year = null;

            if (Month == null)
                Month = now.Month;
            else if (Month == 0)
                Month = null;

            //if (Day == null)
            //    Day = now.Day;
            //else if (Day == 0)
            //    Day = null;

            if (Month.HasValue)
                query = query.Where(c => c.CreatedDate.Month == Month.Value);

            if (Day.HasValue)
                query = query.Where(c => c.CreatedDate.Day == Day.Value);

            if (Year.HasValue)
                query = query.Where(c => c.CreatedDate.Year == Year.Value);
            ////////////////////

            if (!string.IsNullOrEmpty(name))
            {
                var nameLower = name.ToLower();
                query = query.Where(p => p.Users.Fullname.ToLower().Contains(nameLower));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new ResponseModel.PayrollResultDto
                {
                    Id = c.Id,
                    Name = c.Users.Fullname,
                    Salary = c.Salary,
                    DaysWorked = c.DaysWorked,
                    CreatedDate = c.CreatedDate,
                    Note = c.Note,

                }).ToListAsync();

            return new PagedResult<ResponseModel.PayrollResultDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }
        public async Task<ResponseModel.PayrollResultDto> GetById(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var payroll = await _payrollRepository.GetPayrollById(id);
            if (payroll == null)
                throw new ArgumentException("Không tìm thấy bảng lương");

            var manager = currentUserRoles.Contains("Manager");
            var employee = currentUserRoles.Contains("Employee");

            if (manager)
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");
                if (payroll.Users.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager chỉ có thể truy cập bảng lương của user cùng phòng ban");
            }
            else if (employee)
            {
                if (payroll.UserId != currentUserId)
                    throw new UnauthorizedAccessException("Nhân viện chỉ có thể truy cập bảng lương của mình");
            }

            return new ResponseModel.PayrollResultDto
            {
                Id = payroll.Id,
                Name = payroll.Users.Fullname,
                Salary = payroll.Salary,
                DaysWorked = payroll.DaysWorked,
                CreatedDate = payroll.CreatedDate,
                Note = payroll.Note
            };
        }
        public async Task<string> SoftDeletePayroll(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _payrollRepository.GetPayrollById(id);
                if (existing == null)
                    throw new ArgumentException("Không thể tìm thấy bảng lương");

                var employee = await _userRepository.GetUserInfoAsync(existing.UserId);
                if (employee == null)
                    throw new ArgumentException("Không thể tìm thấy user cho bảng lương này");

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new UnauthorizedAccessException("Manager chỉ có thể xóa bảng lương của user cùng phòng ban");

                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");
                }

                var result = await _payrollRepository.SoftDeletePayroll(id);
                if (result == null) return null;
                result.IsDeleted = true;

                await _payrollRepository.SoftDeletePayroll(result.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Payroll " + id + " deleted";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting payroll. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<PagedResult<ResponseModel.PayrollResultDto>> GetPayrollByUser(Guid? staffId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize)
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
                        throw new ArgumentException("Không tìm thấy người dùng");

                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");


                    // Kiểm tra user được lấy có tồn tại không
                    var findUser = await _userRepository.GetUserInfoAsync(staffId.Value);
                    if (findUser == null)
                        throw new ArgumentException("Không tìm thấy user");

                    if (findUser.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot access checkins from other departments");

                }
                else if (currentUserRoles.Contains("Administrator"))
                {
                    // Admin: bắt buộc phải nhập staffId
                    if (staffId == null || staffId == Guid.Empty)
                        throw new ArgumentException("Please input userId");
                }
                else
                {
                    throw new UnauthorizedAccessException("You do not have permission");
                }

                pageIndex ??= 1;
                pageSize ??= 10;

                var user = await _userRepository.GetUserInfoAsync(staffId.Value);
                if (user == null)
                    throw new ArgumentException("Không tìm thấy user");

                var query = _context.Payrolls
                    .Where(p => !p.IsDeleted && p.UserId == staffId.Value)
                    .Include(p => p.Users);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.PayrollResultDto
                    {
                        Id = c.Id,
                        CreatedDate = c.CreatedDate,
                        Salary = c.Salary,
                        Note = c.Note,
                        Name = c.Users.Fullname ?? null,
                    }).ToListAsync();

                return new PagedResult<ResponseModel.PayrollResultDto>
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

        ////////////////////////////////////////////////////////

        public async Task<ResponseModel.PayrollResultDto> CalculatePayrollAsync(Guid staffId, Guid currentUserId, IList<string> currentUserRoles)
        {
            var staff = await _userRepository.GetActiveUserIdAsync(staffId);
            if (staff == null)
                throw new ArgumentException("Không tìm thấy user");

            if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                if (staff.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager chỉ có thể tạo bảng lương của user cùng phòng ban");
            }

            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            // Lấy payroll hiện tại (nếu có)
            var existingPayroll = await _context.Payrolls
                .FirstOrDefaultAsync(p => p.UserId == staffId && p.CreatedDate.Month == month && p.CreatedDate.Year == year);

            // Lấy dữ liệu checkin
            var checkinsInMonth = await _context.Checkins
                .Where(c => c.UserId == staffId
                    && c.CheckinTime.Year == year
                    && c.CheckinTime.Month == month
                    && !c.IsDeleted)
                .ToListAsync();

            var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
            if (schedule == null)
                throw new ArgumentException("Không tìm thấy thời gian làm việc");

            double totalSalary = 0;
            foreach (var checkin in checkinsInMonth)
            {
                totalSalary += checkin.SalaryPerDay;
                _logger.LogInformation("Tien luong {money}", totalSalary);
            }

            var morningHours = (schedule.EndTimeMorning - schedule.StartTimeMorning).TotalHours;
            var afternoonHours = (schedule.EndTimeAfternoon - schedule.StartTimeAfternoon).TotalHours;
            var fullDayHours = morningHours + afternoonHours;
            var halfDayHours = fullDayHours / 2;

            var lunchBreakHours = (schedule.StartTimeAfternoon - schedule.EndTimeMorning).TotalHours;

            var totalDayWorked = checkinsInMonth
            .Where(c =>
            {
                var totalHours = (c.CheckoutTime - c.CheckinTime).TotalHours;

                double normalWorkedHours;
                if (c.CheckinTime.TimeOfDay < schedule.EndTimeMorning.ToTimeSpan()
                    && c.CheckoutTime.TimeOfDay > schedule.StartTimeAfternoon.ToTimeSpan())
                {
                    normalWorkedHours = totalHours - lunchBreakHours;
                }
                else
                {
                    normalWorkedHours = totalHours;
                }

                return (c.LogStatus != LogStatus.None)
                    && (normalWorkedHours >= halfDayHours);
            })
            .Select(c => c.CheckinTime.Date)
            .Distinct()
            .Count();

            if (existingPayroll != null)
            {
                // Update payroll
                existingPayroll.Salary = totalSalary;
                existingPayroll.DaysWorked = totalDayWorked;
                existingPayroll.Note = $"Updated payroll for {month}/{year}";
                //existingPayroll.CreatedDate = DateTime.Now; 
                _context.Payrolls.Update(existingPayroll);
            }
            else
            {
                // Tạo mới payroll
                existingPayroll = new Payroll
                {
                    Id = Guid.NewGuid(),
                    UserId = staffId,
                    Salary = totalSalary,
                    DaysWorked = totalDayWorked,
                    CreatedDate = DateTime.Now,
                    Note = $"Payroll for month {month}/{year}"
                };
                await _payrollRepository.CreatePayrollAsync(existingPayroll);
            }

            await _context.SaveChangesAsync();

            return new PayrollResultDto
            {
                Id = existingPayroll.Id,
                Name = staff.Fullname,
                DaysWorked = totalDayWorked,
                Salary = totalSalary,
                CreatedDate = existingPayroll.CreatedDate,
                Note = existingPayroll.Note
            };
        }

        public async Task<List<ResponseModel.PayrollResultDto>> CalculatePayrollForAllUsersAsync(Guid currentUserId, IList<string> currentUserRoles)
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            var allUsers = await _context.Users
                .Where(u => !u.IsDeleted)  
                .ToListAsync();

            //var allEmployees = allUsers
            //    .Where(u => u.UserRoles.Any(r => r.Role.Name == "Employee"))
            //    .ToList();

            var payrollResults = new List<ResponseModel.PayrollResultDto>();

            foreach (var staff in allUsers)
            {
                try
                {
                    if (currentUserRoles.Contains("Manager"))
                    {
                        var currentUser = await _context.Users
                            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                        if (currentUser?.DepartmentId == null) continue;

                        if (staff.DepartmentId != currentUser.DepartmentId) continue;
                    }

                    var existingPayroll = await _context.Payrolls
                        .FirstOrDefaultAsync(p => p.UserId == staff.UserId && p.CreatedDate.Month == month && p.CreatedDate.Year == year);

                    var checkinsInMonth = await _context.Checkins
                        .Where(c => c.UserId == staff.UserId
                            && c.CheckinTime.Year == year
                            && c.CheckinTime.Month == month
                            && !c.IsDeleted)
                        .ToListAsync();

                    double totalSalary = checkinsInMonth.Sum(c => c.SalaryPerDay);

                    var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync(); 

                    var morningHours = (schedule.EndTimeMorning - schedule.StartTimeMorning).TotalHours;
                    var afternoonHours = (schedule.EndTimeAfternoon - schedule.StartTimeAfternoon).TotalHours;
                    var fullDayHours = morningHours + afternoonHours;
                    var halfDayHours = fullDayHours / 2;

                    var lunchBreakHours = (schedule.StartTimeAfternoon - schedule.EndTimeMorning).TotalHours;

                    var totalDayWorked = checkinsInMonth
                    .Where(c =>
                    {
                        var totalHours = (c.CheckoutTime - c.CheckinTime).TotalHours;

                        double normalWorkedHours;
                        if (c.CheckinTime.TimeOfDay < schedule.EndTimeMorning.ToTimeSpan()
                            && c.CheckoutTime.TimeOfDay > schedule.StartTimeAfternoon.ToTimeSpan())
                        {
                            normalWorkedHours = totalHours - lunchBreakHours;
                        }
                        else
                        {
                            normalWorkedHours = totalHours;
                        }

                        return (c.LogStatus != LogStatus.None)
                            && (normalWorkedHours >= halfDayHours);
                    })
                    .Select(c => c.CheckinTime.Date)
                    .Distinct()
                    .Count();

                    if (existingPayroll != null)
                    {
                        existingPayroll.Salary = totalSalary;
                        existingPayroll.DaysWorked = totalDayWorked;
                        existingPayroll.Note = $"Updated payroll for {month}/{year}";
                        _context.Payrolls.Update(existingPayroll);
                    }
                    else
                    {
                        existingPayroll = new Payroll
                        {
                            Id = Guid.NewGuid(),
                            UserId = staff.UserId,
                            Salary = totalSalary,
                            DaysWorked = totalDayWorked,
                            CreatedDate = DateTime.Now,
                            Note = $"Payroll for month {month}/{year}"
                        };
                        await _payrollRepository.CreatePayrollAsync(existingPayroll);
                    }

                    payrollResults.Add(new ResponseModel.PayrollResultDto
                    {
                        Id = existingPayroll.Id,
                        Name = staff.Fullname,
                        DaysWorked = totalDayWorked,
                        Salary = totalSalary,
                        CreatedDate = existingPayroll.CreatedDate,
                        Note = existingPayroll.Note
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating payroll for user {UserId}", staff.UserId);
                    continue;
                }
            }

            await _context.SaveChangesAsync();
            return payrollResults;
        }


        //public async Task<ResponseModel.PayrollResultDto> CalculatePayrollAsync(Guid staffId, Guid currentUserId, IList<string> currentUserRoles)
        //{
        //    var staff = await _userRepository.GetActiveUserIdAsync(staffId);

        //    if (staff == null)
        //        throw new ArgumentException("Không tìm thấy user");

        //    if (currentUserRoles.Contains("Manager"))
        //    {
        //        var currentUser = await _context.Users
        //            .Include(u => u.Department)
        //            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        //        if (currentUser == null)
        //            throw new ArgumentException("Không thể tìm thấy user hiện tại");

        //        if (currentUser.DepartmentId == null)
        //            throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

        //        if (staff.DepartmentId != currentUser.DepartmentId)
        //            throw new UnauthorizedAccessException("You can only calculate payrolls for User in your department");
        //    }

        //    int month = DateTime.Now.Month;
        //    int year = DateTime.Now.Year;

        //    if (await _payrollRepository.ExistsPayrollForMonth(staffId, month, year))
        //        throw new InvalidOperationException("Payroll for this month already exists");

        //    var checkinsInMonth = await _context.Checkins
        //        .Where(c => c.UserId == staffId
        //            && c.CheckinMorning.Year == year
        //            && c.CheckinMorning.Month == month
        //            && !c.IsDeleted)
        //        .ToListAsync();

        //    var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
        //    if (schedule == null)
        //        throw new ArgumentException("Schedule not found");

        //    double totalSalary = 0;

        //    foreach (var checkin in checkinsInMonth)
        //    {
        //        totalSalary += checkin.SalaryPerDay;
        //        _logger.LogInformation("Tien luong {money}", totalSalary);
        //    }

        //    var totalDayWorked = checkinsInMonth
        //        .Where(p => (p.CheckinMorningStatus != LogStatus.None && p.CheckoutMorningStatus != LogStatus.None
        //                    && p.CheckinMorningStatus != LogStatus.Absent && p.CheckoutMorningStatus != LogStatus.Absent)
        //                    || (p.CheckinAfternoonStatus != LogStatus.None && p.CheckoutAfternoonStatus != LogStatus.None
        //                    && p.CheckinAfternoonStatus != LogStatus.Absent && p.CheckoutAfternoonStatus != LogStatus.Absent))
        //        .Select(c => c.CheckinMorning.Date)
        //        .Distinct()
        //        .Count();

        //    var payroll = new Payroll
        //    {
        //        Id = Guid.NewGuid(),
        //        UserId = staffId,
        //        Salary = totalSalary,
        //        DaysWorked = totalDayWorked,
        //        CreatedDate = DateTime.Now,
        //        Note = $"Lương tháng {month}/{year}"
        //    };

        //    await _payrollRepository.CreatePayrollAsync(payroll);
        //    await _context.SaveChangesAsync();

        //    return new PayrollResultDto
        //    {
        //        Id = payroll.Id,
        //        Name = payroll.Users.Fullname,
        //        DaysWorked = totalDayWorked,
        //        Salary = totalSalary,
        //        CreatedDate = payroll.CreatedDate,
        //        Note = payroll.Note
        //    };
        //}
    }
}
