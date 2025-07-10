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
        public DashboardService(AppDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public async Task<DashboardOverviewDto> GetOverviewAsync(ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            //var role = Claim(ClaimTypes.Role, user.Role.ToString() ?? "");
            var departmentIdStr = user.FindFirst("DepartmentId")?.Value;
            Guid? departmentId = null;
            if (Guid.TryParse(departmentIdStr, out var depId))
                departmentId = depId;

            bool isAdmin = role == "Administrator";
            bool isManager = role == "Manager";


            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new ArgumentException("Không tìm thấy UserId hợp lệ trong claim");
            var currentUser = await _userRepository.GetActiveUserIdAsync(userId);
            if (isManager && currentUser.DepartmentId == null)
                throw new ArgumentException("Manager chưa có phòng ban");

            // Lấy danh sách nhân viên
            var employeesQuery = _context.Users.AsQueryable();
            if (isManager && departmentId.HasValue)
            {
                employeesQuery = employeesQuery.Where(u => u.DepartmentId == departmentId.Value);
            }

            var totalEmployees = await employeesQuery.CountAsync();
            var activeEmployees = await employeesQuery.CountAsync(u => u.IsActive);

            // Tổng số phòng ban
            var totalDepartments = isAdmin
                ? await _context.Departments.CountAsync(d => !d.isDeleted)
                : 1; // Manager chỉ quản lý 1 phòng ban

            // Tổng số chức vụ
            var totalPositions = isAdmin
                ? await _context.Positions.CountAsync(p => !p.IsDeleted)
                : await _context.Positions.CountAsync(p => p.DepartmentId == departmentId && !p.IsDeleted);

            Console.WriteLine($"isAdmin: {isAdmin}, isManager: {isManager}, departmentId: {departmentId}");

            var today = DateTime.Today;

            // Check-in hôm nay
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

            // Ngày nghỉ sắp tới (mọi role đều xem)
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

