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

        [HttpGet/*, Authorize*/]
        public async Task<IActionResult> GetAllPayrolls(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _payrollService.GetAllPayrolls(name, pageIndex, pageSize);

                return Ok(ApiResponse<PagedResult<ResponseModel.PayrollDto>>.ReturnResult("Get list payroll success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving payrolls");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        [HttpPost("calculate")/*, Authorize*/]
        public async Task<IActionResult> CalculatePayroll(Guid staffId)
        {
            try
            {
                var result = await _payrollService.CalculatePayrollAsync(staffId);
                return Ok(ApiResponse<ResponseModel.PaidPayroll>.ReturnResult("Calculate payroll success", result, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while calculating payroll");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        [HttpDelete/*, Authorize*/]
        public async Task<IActionResult> DeletePayroll(Guid id)
        {
            try
            {
                var result = await _payrollService.SoftDeletePayroll(id);
                //if (result == null) return BadRequest("Không thể xóa payroll " + id);
                return Ok(ApiResponse<string>.ReturnResult("Delete payroll success", result, 200));
            }
            catch {
                _logger.LogError("An error occurred while deleting payroll");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find payroll id", StatusCode = 500 });
            }
        }

        [HttpGet("Employee")/*, Authorize*/]
        public async Task<IActionResult> GetPayrollByStaff(Guid staffId, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _payrollService.GetPayrollByStaff(staffId, pageIndex, pageSize);

                return Ok(ApiResponse<PagedResult<ResponseModel.PayrollDto>>.ReturnResult("Get list payroll by staff success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving payrolls by staff");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
    }
}
