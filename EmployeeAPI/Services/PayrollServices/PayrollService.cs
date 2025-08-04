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
using System.ComponentModel.Design;
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

        public async Task<PagedResult<ResponseModel.PayrollResultDto>> GetAllPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, Guid? companyId/*, int? Day*/, int? Month, int? Year, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Payrolls
                .Where(p => p.Users.Role == RoleType.Manager || p.Users.Role == RoleType.Employee)
                .Include(p => p.Users)
                .Where(p => !p.IsDeleted);


            if (currentUserRoles.Contains("SystemAdmin"))
            {
                if (companyId.HasValue)
                {
                    query = query.Where(p => p.Users.CompanyId == companyId.Value);
                }
            }
            else if (currentUserRoles.Contains("Administrator"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser?.CompanyId == null)
                    throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ quản trị hệ thống.");

                query = query.Where(p => p.Users.CompanyId == currentUser.CompanyId);
            }
            else if (currentUserRoles.Contains("Manager"))
            {
                var manager = await _context.Users.FindAsync(currentUserId);
                if (manager == null)
                    throw new ArgumentException("Không thể tìm thấy người dùng hiện tại");

                if (manager.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                var departmentId = manager.DepartmentId;

                query = query.Where(p => p.Users.DepartmentId == departmentId);
            }
            else if( currentUserRoles.Contains("Employee"))
            {
                var employee = await _context.Users.FindAsync(currentUserId);
                if (employee == null)
                    throw new ArgumentException("Employee not found");
                query = query.Where(p => p.UserId == employee.UserId);
            }

            ///////////////////
            if (Month.HasValue)
                query = query.Where(c => c.PayrollMonth == Month.Value);

            if (Year.HasValue)
                query = query.Where(c => c.PayrollYear == Year.Value);
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
                    UserId = c.UserId,
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

            if (currentUserRoles.Contains("Administrator"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser?.CompanyId == null)
                    throw new ArgumentException("Admin chưa có công ty.");

                if (payroll.Users.CompanyId != currentUser.CompanyId)
                    throw new UnauthorizedAccessException("Admin chỉ có thể xem bảng lương của user cùng công ty.");
            }
            else if (manager)
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
                UserId = payroll.UserId,
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
        public async Task<ResponseModel.PayrollResultDto> CalculatePayrollAsync(Guid staffId, int Month, int Year, Guid currentUserId, IList<string> currentUserRoles)
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


            int month = Month;
            int year = Year;

            if (month < 1 || month > 12)
                throw new ArgumentException("Tháng không hợp lệ");

            if (year < 2000 || year > DateTime.Now.Year + 1)
                throw new ArgumentException("Năm không hợp lệ");

            var existingPayroll = await _context.Payrolls
                .FirstOrDefaultAsync(p => p.UserId == staffId && p.PayrollMonth == month && p.PayrollYear == year && !p.IsDeleted);

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

            var morningHours = (schedule.EndTimeMorning - schedule.StartTimeMorning).TotalHours;
            var afternoonHours = (schedule.EndTimeAfternoon - schedule.StartTimeAfternoon).TotalHours;
            var fullDayHours = (morningHours + afternoonHours) - schedule.LogAllowtime;

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
                    && (normalWorkedHours >= fullDayHours);
            })
            .Select(c => c.CheckinTime.Date)
            .Distinct()
            .Count();

            if (existingPayroll != null)
            {
                existingPayroll.Salary = totalSalary;
                existingPayroll.DaysWorked = totalDayWorked;
                existingPayroll.Note = $"Cập nhật chấm công cho tháng {month}/{year}";
                _context.Payrolls.Update(existingPayroll);
            }
            else
            {
                existingPayroll = new Payroll
                {
                    Id = Guid.NewGuid(),
                    UserId = staffId,
                    Salary = totalSalary,
                    DaysWorked = totalDayWorked,
                    CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    Note = $"Tạo chấm công cho tháng {month}/{year}",
                    PayrollMonth = month,
                    PayrollYear = year,
                };
                await _payrollRepository.CreatePayrollAsync(existingPayroll);
            }

            await _context.SaveChangesAsync();

            return new PayrollResultDto
            {
                Id = existingPayroll.Id,
                UserId = existingPayroll.UserId,
                Name = staff.Fullname,
                DaysWorked = totalDayWorked,
                Salary = totalSalary,
                CreatedDate = existingPayroll.CreatedDate,
                Note = existingPayroll.Note
            };
        }

        public async Task<PagedResult<ResponseModel.UserWithPayrollDto>> GetUsersWithPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, Guid? companyId, Guid? departmentId, Guid? positionId/*, int? day*/, int? month, int? year, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _userRepository.GetAll().Where(p => p.Role == RoleType.Manager || p.Role == RoleType.Employee);

            if (currentUserRoles.Contains("Administrator"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser?.CompanyId == null)
                    throw new ArgumentException("Admin chưa có công ty.");

                query = query.Where(u => u.CompanyId == currentUser.CompanyId);
            }
            else if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser?.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban.");

                query = query.Where(u => u.DepartmentId == currentUser.DepartmentId);
            }
            else if (currentUserRoles.Contains("Employee"))
            {
                query = query.Where(u => u.UserId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameLower = name.ToLower();
                query = query.Where(u => u.Fullname.ToLower().Contains(nameLower));
            }

            if (companyId.HasValue)
                query = query.Where(u => u.CompanyId == companyId);
            if (departmentId.HasValue)
                query = query.Where(u => u.DepartmentId == departmentId);
            if (positionId.HasValue)
                query = query.Where(u => u.PositionId == positionId);

            var totalCount = await query.CountAsync();

            var items = await query
                //.OrderByDescending(p => p.CreatedDate)
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(u => new ResponseModel.UserWithPayrollDto
                {
                    UserId = u.UserId,
                    Fullname = u.Fullname,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    ImageUrl = u.ImageUrl,
                    Payrolls = u.Payrolls
                    .Where(p => !p.IsDeleted &&
                    (!month.HasValue || p.PayrollMonth == month.Value) &&
                    (!year.HasValue || p.PayrollYear == year.Value) /*&&
                    (!day.HasValue || p.CreatedDate.Day == day.Value)*/)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(p => new ResponseModel.PayrollResultDto
                    {
                        Id = p.Id,
                        Salary = p.Salary,
                        DaysWorked = p.DaysWorked,
                        CreatedDate = p.CreatedDate,
                        Note = p.Note,
                        Name = u.Fullname
                    }).ToList()
                }).ToListAsync();

            return new PagedResult<ResponseModel.UserWithPayrollDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }
    }
}
