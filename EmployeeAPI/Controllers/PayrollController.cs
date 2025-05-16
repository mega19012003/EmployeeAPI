using EmployeeAPI.Services.PayrollServices;
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

        [HttpGet]
        public async Task<IActionResult> GetAllPayrolls()
        {
            var results = await _payrollService.GetAllPayrolls();
            if (results == null) return NotFound();
            return Ok(results);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetPayrollById(Guid id)
        {
            var result = await _payrollService.GetPayrollById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayroll(ResponseModel.CreatePayroll dto)
        {
            var result = await _payrollService.CreatePayroll(dto);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePayroll(ResponseModel.UpdatePayroll dto)
        {
            var result = await _payrollService.UpdatePayroll(dto);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePayroll(Guid id)
        {
            var result = await _payrollService.SoftDeletePayroll(id);
            if (result == null) return BadRequest("Không thể xóa payroll " + id);
            return Ok(result);
        }

        [HttpGet("Employee")]
        public async Task<IActionResult> GetCheckinsByStaffAndMonth(Guid staffId, int year, int month)
        {
            var results = await _payrollService.GetCheckinsByStaffAndMonthAsync(staffId, year, month);
            if (results == null)
            {
                return NotFound();
            }
            return Ok(results);
        }

        [HttpPost("calculate")]
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
    }
}
