using System.Security.Claims;
using EmployeeAPI.Attributes;
using EmployeeAPI.Base;
using EmployeeAPI.Services.PayrollServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(7)]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(IPayrollService payrollService, ILogger<PayrollController> logger)
        {
            _payrollService = payrollService;
            _logger = logger;
        }

        /// <summary>
        /// lấy toàn bộ danh sách chấm công, manager chỉ dc phép lấy danh sách theo phòng ban của mình
        /// </summary>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAllPayrolls(string? name, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _payrollService.GetAllPayrolls(currentUserId, currentRoles, name, pageIndex, pageSize);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.PayrollResultDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.PayrollResultDto>>.ReturnResult("Get list payroll success", pagedResult, 200));
        }

        /// <summary>
        /// Tình chấm công cho nhân viên, do admin/manager xử lý
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePayroll(Guid userId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var pagedResult = await _payrollService.CalculatePayrollAsync(userId, currentUserId, currentRoles);
            return Ok(ApiResponse<ResponseModel.PaidPayrollDto>.ReturnResult("Calculate payroll success", pagedResult, 200));
        }

        /// <summary>
        /// Xóa chấm công, do admin/manager xử lý
        /// </summary>
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

        /// <summary>
        /// Lấy danh sách chấm công cho nhân viên, manager chỉ phép lấy nhân viên thuộc phòng ban của mình, employee chỉ dc lấy danh sách của bản thân
        /// </summary>
        [Authorize]
        [HttpGet("employee")]
        public async Task<IActionResult> GetPayrollByStaff(Guid userId, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _payrollService.GetPayrollByUser(userId, currentUserId, currentRoles, pageIndex, pageSize);
            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.PayrollResultDto>>.ReturnResult("No result", pagedResult, 200));
            return Ok(ApiResponse<PagedResult<ResponseModel.PayrollResultDto>>.ReturnResult("Get list payroll by User success", pagedResult, 200));
        }
    }
}

