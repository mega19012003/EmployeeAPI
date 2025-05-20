using Microsoft.AspNetCore.Mvc;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Services.PositionServices;
using Microsoft.AspNetCore.Authorization;
using EmployeeAPI.Base;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;
        private readonly ILogger<PositionController> _logger;

        public PositionController(IPositionService positionService, ILogger<PositionController> logger)
        {
            _positionService = positionService;
            _logger = logger;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetAllPositions(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _positionService.GetAllAsync(name, pageIndex, pageSize);

                if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }

                return Ok(new ApiResponse<PagedResult<ResponseModel.PositionDTO>>
                {
                    Message = "Get list position success",
                    Data = pagedResult,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    Message = "An error occurred while retrieving positions",
                    Data = ex.Message,
                    StatusCode = 500
                };
                return StatusCode(500, response);
            }
        }

        /*[HttpGet("id"), Authorize]
        public async Task<IActionResult> GetPositionById(Guid id)
        {
            var position = await _positionService.GetByIdAsync(id);
            if (position == null) return NotFound();
            return Ok(position);
        }*/

        [HttpPost, Authorize]
        public async Task<IActionResult> AddPosition([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) return BadRequest("Position name cannot be empty");
                var result = await _positionService.AddAsync(name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in AddPosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        [HttpPut, Authorize]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromQuery] string newName)
        {
            try
            {
                if (id == Guid.Empty || string.IsNullOrWhiteSpace(newName)) return BadRequest("Invalid input");

                var result = await _positionService.UpdateAsync(id, newName);
                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UpdatePosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        [HttpDelete, Authorize]
        public async Task<IActionResult> SoftDeletePosition([FromQuery] Guid id)
        {
            try
            {
                var result = await _positionService.SoftDeleteAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SoftDeletePosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        [HttpGet("Employee"), Authorize]
        public async Task<IActionResult> GetEmployeeByPosition(string searchTerm, int? pageSize, int? pageIndex)
        {
            try
            {
                var positions = await _positionService.GetStaffByPositionAsync(searchTerm, pageSize, pageIndex);
                if (!positions.Any())
                    return NotFound("Không tìm thấy vị trí hoặc nhân viên phù hợp.");

                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetEmployeeByPosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }
    }
}
