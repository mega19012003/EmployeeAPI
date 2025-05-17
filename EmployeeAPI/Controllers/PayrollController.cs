using EmployeeAPI.Services.PayrollServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;
        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetAllPayrolls()
        {
            var results = await _payrollService.GetAllPayrolls();
            if (results == null) return NotFound();
            return Ok(results);
        }

        /*[HttpGet("id")]
        public async Task<IActionResult> GetPayrollById(Guid id)
        {
            var result = await _payrollService.GetPayrollById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }*/

        [HttpPost("calculate"), Authorize]
        public async Task<IActionResult> CalculatePayroll(Guid staffId)
        {
            try
            {
                var result = await _payrollService.CalculatePayrollAsync(staffId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete, Authorize]
        public async Task<IActionResult> DeletePayroll(Guid id)
        {
            var result = await _payrollService.SoftDeletePayroll(id);
            if (result == null) return BadRequest("Không thể xóa payroll " + id);
            return Ok(result);
        }

        [HttpGet("Employee")]
        public async Task<IActionResult> GetPayrollByStaff(Guid staffId)
        {
            var results = await _payrollService.GetPayrollByStaff(staffId);
            if (results == null)
            {
                return NotFound();
            }
            return Ok(results);
        }

    }
}
