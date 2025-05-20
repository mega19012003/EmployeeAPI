using EmployeeAPI.Base;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Services.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        [HttpGet, Authorize]
        /*public async Task<IActionResult> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm)
        {
            var staff = await _staffService.GetAllAsync(pageSize, pageIndex, SearchTerm);
            return Ok(staff);
        }*/
        public async Task<IActionResult> GetAllAsync(string? SearchTerm, int? pageSize, int? pageIndex)
        {
            try
            {
                var pagedResult = await _staffService.GetAllAsync(SearchTerm, pageSize, pageIndex);

                if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }

                return Ok(new ApiResponse<PagedResult<ResponseModel.StaffDto>>
                {
                    Message = "Get list staff success",
                    Data = pagedResult,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    Message = "An error occurred while retrieving staff",
                    Data = ex.Message,
                    StatusCode = 500
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("Id"), Authorize]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return Ok(staff);
        }

        [HttpPost]
        [Consumes("multipart/form-data"), Authorize]
        public async Task<IActionResult> AddAsync([FromForm] ResponseModel.CreateStaff dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid data.");
            }
            try
            {
                var result = await _staffService.AddAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in AddAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        [HttpPut("id")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAsync([FromForm] ResponseModel.UpdateStaff dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid data.");
            }
            try
            {
                var result = await _staffService.UpdateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UpdateAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        //[HttpPut("delete")]
        [HttpDelete, Authorize]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid Id)
        {
            try
            {
                var result = await _staffService.SoftDeleteAsync(Id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SoftDeleteAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        [HttpGet("name"), Authorize]
        public async Task<IActionResult> GetByNameAsync(string name, int? pageSize, int? pageIndex)
        {
            var result = await _staffService.GetByNameAsync(name, pageSize, pageIndex);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
