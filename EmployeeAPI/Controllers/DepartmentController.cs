using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Services.DepartmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IStaffRepository _staffRepository;

        public DepartmentController(IDepartmentService departmentService, IStaffRepository staffRepository)
        {
            _departmentService = departmentService;
            _staffRepository = staffRepository;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _departmentService.GetAllAsync();
            return Ok(result);
        }

        /*[HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }*/

        [HttpPost, Authorize]
        public async Task<IActionResult> AddDepartment([FromQuery] String Name)
        {
            if (Name == null)
            {
                return BadRequest("Department Name cannot be null");
            }
            var result = await _departmentService.AddAsync(Name);
            return Ok(result);
        }

        [HttpPut, Authorize]
        public async Task<IActionResult> UpdateDepartment([FromQuery] Guid id, [FromQuery] string newName)
        {
            if (id == null || newName == null)
            {
                return BadRequest("Invalid Department data");
            }
            //var existingDepartment = await _departmentService.GetByIdAsync(id);
            var result = await _departmentService.UpdateAsync(id, newName);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpDelete, Authorize]
        public async Task<IActionResult> SoftDeleteDepartment(Guid id)
        {
            if (id == null) return BadRequest("Id không hợp lệ hoặc tồn tại");

            var result = await _departmentService.SoftDeleteAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }
        
        /*[HttpGet("name"), Authorize]
        public async Task<IActionResult> GetDepartmentByName(string name)
        {
            var department = await _departmentService.GetDepartmentByName(name);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }*/
        [HttpGet("Employee"), Authorize]
        public async Task<IActionResult> GetEmployeeByPosition(string searchTerm, int? pageSize, int? pageIndex)
        {
            var positions = await _departmentService.GetStaffByDepartmentAsync(searchTerm, pageSize, pageIndex);
            if (!positions.Any())
                return NotFound("Không tìm thấy phòng ban hoặc nhân viên phù hợp.");

            return Ok(positions);
        }
    }
}
