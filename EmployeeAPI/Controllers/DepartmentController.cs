using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Services.DepartmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.AuthServices.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(IDepartmentService departmentService, IAuthRepository authRepository, ILogger<DepartmentController> logger)
        {
            _departmentService = departmentService;
            _authRepository = authRepository;
            _logger = logger;
        }

        [HttpGet/*, Authorize*/]
        public async Task<IActionResult> GetAll(string? name, int? pageIndex, int? pageSize)
        {
            /*var result = await _departmentService.GetAllAsync(name, pageIndex, pageSize);
            return Ok(result);*/
            try
            {
                var pagedResult = await _departmentService.GetAllAsync(name, pageIndex, pageSize);

                /*if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }*/

                return Ok(ApiResponse<PagedResult<ResponseModel.DepartmentDto>>.ReturnResult("Get list department success", pagedResult, 200));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving departments");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
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

        [HttpPost/*, Authorize*/]
        public async Task<IActionResult> AddDepartment([FromQuery] String Name)
        {
            try
            {
                if (Name == null)
                {
                    return BadRequest("Department Name cannot be null");
                }
                var result = await _departmentService.AddAsync(Name);

                return Ok(ApiResponse<ResponseModel.CreateDepartment>.ReturnResult("Department added success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while adding the department");
                return StatusCode(400, new { Message = "Department cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while adding the department");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the department");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        [HttpPut/*, Authorize*/]
        public async Task<IActionResult> UpdateDepartment([FromQuery] Guid id, [FromQuery] string newName)
        {
            try
            {
                //var existingDepartment = await _departmentService.GetByIdAsync(id);
                var result = await _departmentService.UpdateAsync(id, newName);

                return Ok(ApiResponse<ResponseModel.UpdateDepartment>.ReturnResult("Updated Department Success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while adding the department");
                return StatusCode(400, new { Message = "Department cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while updating the department");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the department");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }


        [HttpDelete/*, Authorize*/]
        public async Task<IActionResult> SoftDeleteDepartment(Guid id)
        {
            try
            {
                //if (id == null) return BadRequest("Id không hợp lệ hoặc tồn tại");

                var result = await _departmentService.SoftDeleteAsync(id);
                if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("", result, 400));

                return Ok(ApiResponse<string>.ReturnResult("Delete department success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while deleting the department");
                return StatusCode(400, new { Message = "Department cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while deleting the department");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the department");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        [HttpGet("Employee-nhớ sửa lại")/*, Authorize*/]
        public async Task<IActionResult> GetEmployeeByDepartment(string DepartmentName, int? pageSize, int? pageIndex)
        {
            try
            {
                var pagedResult = await _departmentService.GetStaffByDepartmentAsync(DepartmentName, pageSize, pageIndex);
                if (pagedResult == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the department id", null, 404));

                return Ok(ApiResponse<PagedResult<UserFilter>>.ReturnResult("Get list staff by department success", pagedResult, 200));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while retrieving employees by department");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving employees by department");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
    }
}
