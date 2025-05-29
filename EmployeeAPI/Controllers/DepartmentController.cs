using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Services.DepartmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EmployeeAPI.Services.UserService;
using ResponseModel = EmployeeAPI.Services.DepartmentServices.ResponseModel;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentController> _logger;
        private readonly IUserService _userService;

        public DepartmentController(IDepartmentService departmentService, IUserService userService, ILogger<DepartmentController> logger)
        {
            _departmentService = departmentService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách phòng ban, manager/employee ko dc phép truy cập
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? name, int? pageIndex, int? pageSize)
        {
            /*var result = await _departmentService.GetAllAsync(name, pageIndex, pageSize);
            return Ok(result);*/
            try
            {
                var pagedResult = await _departmentService.GetAllAsync(name, pageIndex, pageSize);
                return Ok(ApiResponse<PagedResult<ResponseModel.DepartmentDto>>.ReturnResult("Get list department success", pagedResult, 200));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving departments");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Thêm phòng ban, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
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

        /// <summary>
        /// Cập nhật phòng ban,  do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
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

        /// <summary>
        /// Xóa phòng ban, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("id")]
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

        /// <summary>
        /// lấy danh sách nhân viên theo phòng ban, manager sẽ lấy nhan viên theo phòng ban của mình
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeByDepartment(Guid departmentId, int? pageSize, int? pageIndex)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid user");

                Guid? id = null;

                if (userRole == "Manager")
                {
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null)
                        return Unauthorized("User not found");
                    id = user.DepartmentId;

                }
                if (userRole == "Administrator")
                {
                    id = departmentId;
                }

                var pagedResult = await _departmentService.GetStaffByDepartmentAsync(id, pageSize, pageIndex);
                if (pagedResult == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the department id", null, 404));

                return Ok(ApiResponse<PagedResult<ResponseModel.UserFilter>>.ReturnResult("Get list staff by department success", pagedResult, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while retrieving employees by department");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Get list staffs failed", argEx.InnerException?.Message ?? argEx.Message, 400));
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

        /// <summary>
        /// lấy danh sách chức vụ có trong phòng ban, manager sẽ lấy chức vụ theo phòng ban của mình
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("position")]
        public async Task<IActionResult> GetPositionsByDepartmentAsync(Guid DepartmentId, int? pageSize, int? pageIndex)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid user");

                Guid? id = null;

                if (userRole == "Manager")
                {
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null)
                        return Unauthorized("Position not found");
                    id = user.DepartmentId;

                }
                if (userRole == "Administrator")
                {
                    id = DepartmentId;
                }

                var pagedResult = await _departmentService.GetListPositionAsync(id, pageSize, pageIndex);
                if (pagedResult == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the department id", null, 404));

                return Ok(ApiResponse<PagedResult<PositionByDepartment>>.ReturnResult("Get list posistion by department success", pagedResult, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while retrieving position by department");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Get list position failed", argEx.InnerException?.Message ?? argEx.Message, 400));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while retrieving position by department");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving position by department");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
    }
}
