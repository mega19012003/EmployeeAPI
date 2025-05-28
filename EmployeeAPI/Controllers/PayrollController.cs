using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Services.PayrollServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("Invalid user ID");

                var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var pagedResult = await _payrollService.GetAllPayrolls(currentUserId, currentRoles, name, pageIndex, pageSize);
                //var pagedResult = await _payrollService.GetAllPayrolls(name, pageIndex, pageSize);

                return Ok(ApiResponse<PagedResult<ResponseModel.PayrollDto>>.ReturnResult("Get list payroll success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving payrolls");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Tình chấm công cho nhân viên, do admin/manager xử lý
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePayroll(Guid staffId)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("Invalid user ID");

                var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
               // var pagedResult = await _payrollService.GetAllPayrolls(currentUserId, currentRoles, name, pageIndex, pageSize);
                var pagedResult = await _payrollService.CalculatePayrollAsync(staffId, currentUserId, currentRoles);
                return Ok(ApiResponse<ResponseModel.PaidPayroll>.ReturnResult("Calculate payroll success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while calculating payroll");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Xóa chấm công, do admin/manager xử lý
        /// </summary>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpDelete]
        public async Task<IActionResult> DeletePayroll(Guid id)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("Invalid user ID");

                var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _payrollService.SoftDeletePayroll(id, currentUserId, currentRoles);
                //if (result == null) return BadRequest("Không thể xóa payroll " + id);
                return Ok(ApiResponse<string>.ReturnResult("Delete payroll success", result, 200));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting payroll");
                return NotFound(new { Message = "Payroll not found", Detail = "Cannot find payroll id", StatusCode = 404 });
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while deleting payroll");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Lấy danh sách chấm công cho nhân viên, manager chỉ phép lấy nhân viên thuộc phòng ban của mình, employee chỉ dc lấy danh sách của bản thân
        /// </summary>
        [Authorize]
        [HttpGet("Employee")]
        public async Task<IActionResult> GetPayrollByStaff(Guid? staffId, int? pageIndex, int? pageSize)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("Invalid user ID");

                var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var pagedResult = await _payrollService.GetPayrollByUser(staffId.Value, currentUserId, currentRoles, pageIndex, pageSize);

                return Ok(ApiResponse<PagedResult<ResponseModel.PayrollDto>>.ReturnResult("Get list payroll by staff success", pagedResult, 200));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting payroll");
                return StatusCode(400, new { Message = "Payroll not found", Detail = ex.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving payrolls by staff");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

    }
}

