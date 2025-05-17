using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckinController : ControllerBase
    {
        private readonly ICheckinService _service;

        public CheckinController(ICheckinService service)
        {
            _service = service;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        /*[HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var checkin = await _service.GetByIdAsync(id);
            if (checkin == null) return NotFound();
            return Ok(checkin);
        }*/

        [HttpPost, Authorize]
        public async Task<IActionResult> Create([FromBody] ResponseModel.CreateCheckin dto)
        {
            var created = await _service.CreateAsync(dto);
            if (created == null) return BadRequest("Nhân viên không tồn tại hoặc đã Check-in trong ngày");
            return Ok(created);
        }

        [HttpPut, Authorize]
        public async Task<IActionResult> Update([FromBody] ResponseModel.UpdateCheckin dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _service.UpdateAsync(dto);
            return updated != null ? Ok(updated) : NotFound();
        }

        [HttpDelete, Authorize]
        public async Task<IActionResult> SoftDeleteAsync(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("employee"), Authorize]
        public async Task<IActionResult> GetCheckinsByStaff(Guid staffId)
        {
            var checkins = await _service.GetCheckinByStaffAsync(staffId);
            if (checkins == null) return NotFound();
            return Ok(checkins);
        }
    }
}
