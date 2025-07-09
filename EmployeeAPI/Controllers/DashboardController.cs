using EmployeeAPI.Services.CheckinServices;
using EmployeeAPI.Services.Dashboards;
using EmployeeAPI.Services.DepartmentServices;
using EmployeeAPI.Services.HolidayServices;
using EmployeeAPI.Services.PayrollServices;
using EmployeeAPI.Services.PositionServices;
using EmployeeAPI.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("overview")]
        [Authorize(Roles = "Administrator, Manager")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _dashboardService.GetOverviewAsync(User);
            return Ok(result);
        }
    }
}
