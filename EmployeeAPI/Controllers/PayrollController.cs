using EmployeeAPI.Attributes;
using EmployeeAPI.Base;
using EmployeeAPI.Services.PayrollServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(9)]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(IPayrollService payrollService, ILogger<PayrollController> logger)
        {
            _payrollService = payrollService;
            _logger = logger;
        }

        // <summary>
        /// Lấy toàn bộ danh sách chấm công, manager chỉ dc phép lấy danh sách theo phòng ban của mình, employee lấy danh sách của bản thân
        // </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllPayrolls(string? Search, Guid? companyId, int? Month, int? Year, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _payrollService.GetAllPayrolls(currentUserId, currentRoles, Search, companyId, Month, Year, pageIndex, pageSize);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.PayrollResultDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.PayrollResultDto>>.ReturnResult("Get list payroll success", pagedResult, 200));
        }

        // <summary>
        /// Lấy chấm công
        // </summary>
        [Authorize]
        [HttpGet("{payrollId}")]
        public async Task<IActionResult> GetPayrollById(Guid payrollId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _payrollService.GetById(payrollId, currentUserId, currentRoles);

            return Ok(ApiResponse<ResponseModel.PayrollResultDto>.ReturnResult("Get list payroll success", pagedResult, 200));
        }

        // <summary>
        /// Tình chấm công cho nhân viên, do admin/manager xử lý
        // </summary>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePayroll(Guid userId, int Month, int Year)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var pagedResult = await _payrollService.CalculatePayrollAsync(userId, Month, Year, currentUserId, currentRoles);
            return Ok(ApiResponse<ResponseModel.PayrollResultDto>.ReturnResult("Calculate payroll success", pagedResult, 200));
        }


        //// <summary>
        ///// Tình chấm công cho toàn bộ nhân viên, do admin/manager xử lý
        //// </summary>
        //[Authorize(Roles = "Administrator,Manager")]
        //[HttpPost("calculateAll")]
        //public async Task<IActionResult> CalculateAllUserPayroll()
        //{
        //    var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
        //        return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

        //    var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        //    var pagedResult = await _payrollService.CalculatePayrollForAllUsersAsync(currentUserId, currentRoles);
        //    return Ok(ApiResponse<List<ResponseModel.PayrollResultDto>>.ReturnResult("Calculate payroll success", pagedResult, 200));
        //}

        // <summary>
        /// Xóa chấm công, do admin/manager xử lý
        // </summary>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpDelete("{payrollId}")]
        public async Task<IActionResult> DeletePayroll(Guid payrollId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _payrollService.SoftDeletePayroll(payrollId, currentUserId, currentRoles);
            return Ok(ApiResponse<string>.ReturnResult("Delete payroll success", result, 200));
        }

        // <summary>
        /// Lấy toàn bộ danh sách người dùng kèm payroll của họ.
        /// Manager chỉ được thấy người trong phòng ban.
        /// Employee chỉ thấy chính mình.
        // </summary>
        [Authorize]
        [HttpGet("user-payrolls")]
        public async Task<IActionResult> GetAllUsersWithPayrolls(string? Search, Guid? companyId, Guid? departmentId, Guid? positionId, int? Month, int? Year, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _payrollService.GetUsersWithPayrolls(currentUserId, currentRoles, Search, companyId, departmentId, positionId, Month, Year, pageIndex, pageSize);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.UserWithPayrollDto>>.ReturnResult("No data found", result, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.UserWithPayrollDto>>.ReturnResult("Get users with payrolls success", result, 200));
        }

    }
}

