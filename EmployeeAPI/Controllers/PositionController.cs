using Microsoft.AspNetCore.Mvc;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Services.PositionServices;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;
        //private readonly IStaffService _staffService; // Staff dùng service tương tự

        public PositionController(IPositionService positionService /*IStaffService staffService*/)
        {
            _positionService = positionService;
            /*_staffService = staffService;*/
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPositions()
        {
            var result = await _positionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPositionById(Guid id)
        {
            var position = await _positionService.GetByIdAsync(id);
            if (position == null) return NotFound();
            return Ok(position);
        }

        [HttpPost]
        public async Task<IActionResult> AddPosition([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Position name cannot be empty");
            var result = await _positionService.AddAsync(name);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromQuery] string newName)
        {
            if (id == Guid.Empty || string.IsNullOrWhiteSpace(newName)) return BadRequest("Invalid input");

            var result = await _positionService.UpdateAsync(id, newName);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPut("softDelete")]
        public async Task<IActionResult> SoftDeletePosition([FromQuery] Guid id)
        {
            var result = await _positionService.SoftDeleteAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /*[HttpGet("employee-not-done")]
        public async Task<IActionResult> GetEmployeesByPosition([FromQuery] string name, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            var employees = await _staffService.GetEmployeeByPosition(name, pageSize, pageIndex);
            if (employees == null) return NotFound();
            return Ok(employees);
        }*/
    }
}
