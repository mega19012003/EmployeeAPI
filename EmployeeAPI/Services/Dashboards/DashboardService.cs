using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static EmployeeAPI.Services.Dashboards.ResponseModel;

namespace EmployeeAPI.Services.Dashboards
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DashboardService> _logger;
        public DashboardService(AppDbContext context, IUserRepository userRepository, ILogger<DashboardService> logger)
        {
            _context = context;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<DashboardOverviewDto> GetOverviewAsync(ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            bool isAdmin = role == "Administrator";
            bool isManager = role == "Manager";

          

            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new ArgumentException("Không tìm thấy UserId hợp lệ trong claim");

            var currentUser = await _userRepository.GetActiveUserIdAsync(userId);
            if (isManager && currentUser.DepartmentId == null)
                throw new ArgumentException("Manager chưa có phòng ban");
            Guid? departmentId = isManager ? currentUser.DepartmentId : null;
            _logger.LogInformation("Getting dashboard overview for user with role: {Role}, DepartmentId: {DepartmentId}", role, departmentId);

            // Tổng số nhân viên
            var employeesQuery = _context.Users.AsQueryable();
            if (isManager && departmentId.HasValue)
            {
                employeesQuery = employeesQuery.Where(u => u.DepartmentId == departmentId.Value && !u.IsDeleted);
            }
            var totalEmployees = await employeesQuery.CountAsync();
            var activeEmployees = await employeesQuery.CountAsync(u => u.IsActive);

            // Tổng số phòng ban
            var departmentsQuery = _context.Departments.Where(d => !d.isDeleted);
            if (isManager && departmentId.HasValue)
            {
                departmentsQuery = departmentsQuery.Where(d => d.Id == departmentId.Value);
            }
            var totalDepartments = await departmentsQuery.CountAsync();

            // Tổng số chức vụ
            var positionsQuery = _context.Positions.Where(p => !p.IsDeleted);
            if (isManager && departmentId.HasValue)
            {
                positionsQuery = positionsQuery.Where(p => p.DepartmentId == departmentId.Value);
            }
            var totalPositions = await positionsQuery.CountAsync();

            var today = DateTime.Today;

            // Tổng số check-in hôm nay
            var checkinsQuery = _context.Checkins.Where(c => c.CheckinTime.Date == today);
            if (isManager && departmentId.HasValue)
            {
                checkinsQuery = checkinsQuery.Where(c => c.Users.DepartmentId == departmentId.Value);
            }
            var totalCheckinsToday = await checkinsQuery.CountAsync();

            // Tổng lương tháng này
            var now = DateTime.Now;
            var payrollQuery = _context.Payrolls
                .Where(p => p.CreatedDate.Month == now.Month && p.CreatedDate.Year == now.Year);
            if (isManager && departmentId.HasValue)
            {
                payrollQuery = payrollQuery.Where(p => p.Users.DepartmentId == departmentId.Value);
            }
            var totalPayrollThisMonth = await payrollQuery.SumAsync(p => (decimal?)p.Salary) ?? 0;

            // Ngày lễ sắp tới: luôn lấy tất cả
            var todayDateOnly = DateOnly.FromDateTime(now);
            var upcomingHolidays = await _context.Holidays
                .Where(h => h.startDate > todayDateOnly)
                .OrderBy(h => h.startDate)
                .Select(h => new UpcomingHolidayDto
                {
                    Name = h.name,
                    Date = h.startDate.ToDateTime(TimeOnly.MinValue)
                })
                .ToListAsync();

            return new DashboardOverviewDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                TotalDepartments = totalDepartments,
                TotalPositions = totalPositions,
                TotalCheckinsToday = totalCheckinsToday,
                TotalPayrollThisMonth = totalPayrollThisMonth,
                UpcomingHolidays = upcomingHolidays
            };
        }
    }
}


