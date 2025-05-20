using EmployeeAPI.Base;
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
        public async Task<IActionResult> GetAllPayrolls(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _payrollService.GetAllPayrolls(name, pageIndex, pageSize);

                if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }

                return Ok(new ApiResponse<PagedResult<ResponseModel.PayrollDto>>
                {
                    Message = "Get list Payroll success",
                    Data = pagedResult,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    Message = "An error occurred while retrieving payrolls",
                    Data = ex.Message,
                    StatusCode = 500
                };
                /*return StatusCode(500, response);
                var results = await _payrollService.GetAllPayrolls(name, pageIndex, pageSize);
            if (results == null) return NotFound();
            return Ok(results);*/
                return StatusCode(500, response);
            }
        }

        /*[HttpGet("id"), Authorize]
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

        [HttpGet("Employee"), Authorize]
        public async Task<IActionResult> GetPayrollByStaff(Guid staffId, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _payrollService.GetPayrollByStaff(staffId, pageIndex, pageSize);

                if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }

                return Ok(new ApiResponse<PagedResult<ResponseModel.PayrollDto>>
                {
                    Message = "Get list staff by payroll success",
                    Data = pagedResult,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    Message = "An error occurred while retrieving staff by payroll",
                    Data = ex.Message,
                    StatusCode = 500
                };

                return StatusCode(500, response);
            }
        }
    }
}
