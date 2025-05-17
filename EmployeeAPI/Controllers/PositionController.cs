using Microsoft.AspNetCore.Mvc;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Services.PositionServices;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetAllPositions()
        {
            var result = await _positionService.GetAllAsync();
            return Ok(result);
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
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Position name cannot be empty");
            var result = await _positionService.AddAsync(name);
            return Ok(result);
        }

        [HttpPut, Authorize]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromQuery] string newName)
        {
            if (id == Guid.Empty || string.IsNullOrWhiteSpace(newName)) return BadRequest("Invalid input");

            var result = await _positionService.UpdateAsync(id, newName);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete, Authorize]
        public async Task<IActionResult> SoftDeletePosition([FromQuery] Guid id)
        {
            var result = await _positionService.SoftDeleteAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("Employee"), Authorize]
        public async Task<IActionResult> GetEmployeeByPosition(string searchTerm, int? pageSize, int? pageIndex)
        {
            var positions = await _positionService.GetStaffByPositionAsync(searchTerm, pageSize, pageIndex);
            if (!positions.Any())
                return NotFound("Không tìm thấy vị trí hoặc nhân viên phù hợp.");

            return Ok(positions);
        }
    }
}
