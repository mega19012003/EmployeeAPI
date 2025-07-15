using EmployeeAPI.Attributes;
using EmployeeAPI.Base;
using EmployeeAPI.Services.CheckinServices;
using EmployeeAPI.Services.Dashboards;
using EmployeeAPI.Services.DepartmentServices;
using EmployeeAPI.Services.HolidayServices;
using EmployeeAPI.Services.PayrollServices;
using EmployeeAPI.Services.PositionServices;
using EmployeeAPI.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static EmployeeAPI.Services.Dashboards.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(3)]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("overview")]
        [Authorize(Roles = "Administrator, Manager, SystemAdmin")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _dashboardService.GetOverviewAsync(User);
            //return Ok(result);
            return Ok(ApiResponse<DashboardOverviewDto>.ReturnResult("Get dashboard success", result, 200));
        }
    }
}
