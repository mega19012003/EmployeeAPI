using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Services.DutyServices;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DutyController : ControllerBase
    {
        private readonly IDutyService _dutyService;

        public DutyController(IDutyService dutyService)
        {
            _dutyService = dutyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int? pageSize, int? pageIndex, string? SearchTerm)
        {
            var result = await _dutyService.GetAllAsync(pageSize, pageIndex, SearchTerm);
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

        [HttpGet("Id")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var duty = await _dutyService.GetByIdAsync(id);
            if (duty == null)
            {
                return NotFound();
            }
            return Ok(duty);
        }

        [HttpPost]
        public async Task<IActionResult> AddDutyAsync(ResponseModel.CreateDuty dto)
        {
            var result = await _dutyService.AddAsync(dto);
            if (dto == null)
            {
                return BadRequest("Invalid data.");
            }
            
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDutyAsync(ResponseModel.UpdateDuty dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid data.");
            }
            var result = await _dutyService.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid id)
        {
            var result = await _dutyService.SoftDeleteAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

    }
}
