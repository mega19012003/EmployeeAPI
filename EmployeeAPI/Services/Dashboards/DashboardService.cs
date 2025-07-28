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
            bool isSystemAdmin = role == "SystemAdmin";

            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new ArgumentException("Không tìm thấy UserId hợp lệ trong claim");

            var currentUser = await _userRepository.GetActiveUserIdAsync(userId);
            if (currentUser == null)
                throw new ArgumentException("Không tìm thấy người dùng hiện tại");

            Guid? departmentId = null;
            Guid? companyId = null;

            if (isManager)
            {
                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban");
                departmentId = currentUser.DepartmentId;
            }
            else if (isAdmin)
            {
                if (currentUser.CompanyId == null)
                    throw new ArgumentException("Admin chưa có công ty");
                companyId = currentUser.CompanyId;
            }

            _logger.LogInformation("Getting dashboard overview for role: {Role}, CompanyId: {CompanyId}, DepartmentId: {DepartmentId}", role, companyId, departmentId);

            // Tổng số công ty
            var companiesQuery = _context.Companies.AsQueryable();
            var totalCompanies = await companiesQuery.CountAsync();

            // Tổng số nhân viên
            var employeesQuery = _context.Users.AsQueryable().Where(u => !u.IsDeleted);
            if (isManager && departmentId.HasValue)
            {
                employeesQuery = employeesQuery.Where(u => u.DepartmentId == departmentId.Value);
            }
            else if (isAdmin && companyId.HasValue)
            {
                employeesQuery = employeesQuery.Where(u => u.CompanyId == companyId.Value);
            }
            // SystemAdmin lấy hết

            var totalEmployees = await employeesQuery.CountAsync();
            var activeEmployees = await employeesQuery.CountAsync(u => u.IsActive);

            // Tổng số phòng ban
            var departmentsQuery = _context.Departments.Where(d => !d.isDeleted);
            if (isManager && departmentId.HasValue)
            {
                departmentsQuery = departmentsQuery.Where(d => d.Id == departmentId.Value);
            }
            else if (isAdmin && companyId.HasValue)
            {
                departmentsQuery = departmentsQuery.Where(d => d.CompanyId == companyId.Value);
            }
            var totalDepartments = await departmentsQuery.CountAsync();

            // Tổng số chức vụ
            var positionsQuery = _context.Positions.Where(p => !p.IsDeleted);
            if (isManager && departmentId.HasValue)
            {
                positionsQuery = positionsQuery.Where(p => p.DepartmentId == departmentId.Value);
            }
            else if (isAdmin && companyId.HasValue)
            {
                positionsQuery = positionsQuery.Where(p => p.Department.CompanyId == companyId.Value);
            }
            var totalPositions = await positionsQuery.CountAsync();

            var today = DateTime.Today;

            // Tổng số check-in hôm nay
            var checkinsQuery = _context.Checkins.Where(c => c.CheckinTime.Date == today);
            if (isManager && departmentId.HasValue)
            {
                checkinsQuery = checkinsQuery.Where(c => c.Users.DepartmentId == departmentId.Value);
            }
            else if (isAdmin && companyId.HasValue)
            {
                checkinsQuery = checkinsQuery.Where(c => c.Users.CompanyId == companyId.Value);
            }
            var totalCheckinsToday = await checkinsQuery.CountAsync();

            // Tổng lương tháng này
            var now = DateTime.Now;
            //var payrollQuery = _context.Payrolls
            //    .Where(p => p.CreatedDate.Month == now.Month && p.CreatedDate.Year == now.Year);
            //if (isManager && departmentId.HasValue)
            //{
            //    payrollQuery = payrollQuery.Where(p => p.Users.DepartmentId == departmentId.Value);
            //}
            //else if (isAdmin && companyId.HasValue)
            //{
            //    payrollQuery = payrollQuery.Where(p => p.Users.CompanyId == companyId.Value);
            //}
            //var totalPayrollThisMonth = await payrollQuery.SumAsync(p => (decimal?)p.Salary) ?? 0;
            // Ngày lễ sắp tới: SystemAdmin/Admin/Manager đều xem được tất cả
            var todayDateOnly = DateOnly.FromDateTime(now);
            var holidayQuery = _context.Holidays
            .Where(h => h.startDate > todayDateOnly);

            if (!isSystemAdmin)
            {
                if (currentUser.CompanyId == null)
                    throw new ArgumentException("Người dùng chưa được gán công ty");

                holidayQuery = holidayQuery.Where(h => h.CompanyId == currentUser.CompanyId.Value);
            }

            var upcomingHolidays = await holidayQuery
                .OrderBy(h => h.startDate)
                .Select(h => new UpcomingHolidayDto
                {
                    Name = h.name,
                    Date = h.startDate.ToDateTime(TimeOnly.MinValue)
                })
                .ToListAsync();

            return new DashboardOverviewDto
            {
                TotalCompanies = totalCompanies,
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                TotalDepartments = totalDepartments,
                TotalPositions = totalPositions,
                TotalCheckinsToday = totalCheckinsToday,
                //TotalPayrollThisMonth = totalPayrollThisMonth,
                //TotalPayrollThisMonth = totalPayrollThisMonth % 1 == 0 ? $"{(long)totalPayrollThisMonth:N0} VNĐ" : $"{totalPayrollThisMonth:N2} VNĐ",
                UpcomingHolidays = upcomingHolidays
            };
        }
    }
}


