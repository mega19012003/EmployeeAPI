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

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm)
        {
            var staff = await _staffService.GetAllAsync(pageSize, pageIndex, SearchTerm);
            return Ok(staff);
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
            var result = await _staffService.AddAsync(dto);
            return Ok(result);
            
        }

        [HttpPut("id")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAsync([FromForm] ResponseModel.UpdateStaff dto)
        {
            var result = await _staffService.UpdateAsync(dto);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        //[HttpPut("delete")]
        [HttpDelete, Authorize]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid Id)
        {
            var result = await _staffService.SoftDeleteAsync(Id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
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
