using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Services.DutyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DutyController : ControllerBase
    {
        private readonly IDutyService _dutyService;
        private readonly ILogger<DutyController> _logger;
        public DutyController(IDutyService dutyService, ILogger<DutyController> logger)
        {
            _dutyService = dutyService;
            _logger = logger;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetAll(string? SearchTerm, int? pageSize, int? pageIndex)
        {
            try
            {
                var result = await _dutyService.GetAllAsync(SearchTerm, pageSize, pageIndex);
                if (pageSize == null || pageSize <= 0)
                {
                    pageSize = 10;
                }
                if (pageIndex == null || pageIndex <= 0)
                {
                    pageIndex = 1;
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("Id"), Authorize]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            try
            {
                var duty = await _dutyService.GetByIdAsync(id);
                if (duty == null)
                {
                    return NotFound();
                }
                return Ok(duty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> AddDutyAsync(ResponseModel.CreateDuty dto)
        {
            try
            {
                var result = await _dutyService.AddAsync(dto);
                if (dto == null)
                {
                    return BadRequest("Invalid data.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut, Authorize]
        public async Task<IActionResult> UpdateDutyAsync(ResponseModel.UpdateDuty dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Invalid data.");
                }
                var result = await _dutyService.UpdateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete, Authorize]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid id)
        {
            try
            {
                var result = await _dutyService.SoftDeleteAsync(id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
