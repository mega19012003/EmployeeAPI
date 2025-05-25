using EmployeeAPI.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using EmployeeAPI.Services.UserService;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public UserController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Cập nhật thông tin người dùng, sẽ do admin chỉnh sửa hết thông tin
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPut("AdminUpdateUser")/*, Authorize*/]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AdminUpdateStaffAsync([FromForm] ResponseModel.AdminUpdateDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var result = await _userService.AdminUpdateStaffAsync(dto);

                return Ok(ApiResponse<ResponseModel.UserDto>.ReturnResult("Update staff success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in UpdateAsync");
                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in UpdateAsync");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UpdateAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Cập nhật thông tin người dùng, sẽ do manager chỉnh, manager ko dc chỉnh role và departmentid sẽ tự gán cho staff
        /// </summary>
        [Authorize(Roles = "Manager")]
        [HttpPut("ManagerUpdateUser")/*, Authorize*/]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ManagerUpdateStaffAsync([FromForm] ResponseModel.ManagerUpdateDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var result = await _userService.ManagerUpdateStaffAsync(dto, dto.UserId);

                return Ok(ApiResponse<ResponseModel.UserDto>.ReturnResult("Update staff success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in UpdateAsync");
                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in UpdateAsync");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UpdateAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Xóa người dùng, sẽ do admin/manager xử lý
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete/*, Authorize*/]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid Id)
        {
            try
            {
                var result = await _userService.SoftDeleteAsync(Id);
                /*if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);*/
                return Ok(ApiResponse<string>.ReturnResult("Soft delete staff success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in SoftDeleteAsync");
                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeleteAsync");
                return BadRequest(new { Message = "Database update failed", Detail = dbEx.InnerException?.Message ?? dbEx.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SoftDeleteAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Staff Id", StatusCode = 500 });
            }
        }

        /// <summary>
        /// Admin Lấy toàn bộ thông tin người dùng, manager lấy danh sách theo phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllUserAsync(string? Name, Guid? departmentId, int? pageSize, int? pageIndex)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid currentUserId))
                {
                    return Unauthorized("Cannot determine current user.");
                }

                var isManager = User.IsInRole("Manager");
                var isAdmin = User.IsInRole("Administrator");

                // Nếu là Manager và không phải Admin, thì tự gán departmentId nếu chưa truyền
                if (isManager && !isAdmin)
                {
                    var result = await _userService.GetByIdAsync(currentUserId);
                    if (result == null)
                        return Unauthorized("Current user not found");

                    if (!result.DepartmentId.HasValue)
                        return BadRequest("Manager chưa có phòng ban.");

                    departmentId = result.DepartmentId;
                }

                var pagedResult = await _userService.GetAllAsync(Name, departmentId, pageIndex, pageSize);
                return Ok(ApiResponse<PagedResult<UserDto>>.ReturnResult("Get list user success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllUserAsync");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.ToString() });
            }
        }

        /// <summary>
        /// Admin Lấy toàn bộ thông tin người dùng, manager lấy user theo phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("id")] 
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid currentUserId))
                    return Unauthorized("Cannot determine current user.");

                var isManager = User.IsInRole("Manager");
                var isAdmin = User.IsInRole("Administrator");

                var result = await _userService.GetByIdAsync(id);
                if (result == null)
                    return NotFound("User not found.");


                if (isManager)
                {
                    var currentUser = await _userService.GetByIdAsync(currentUserId);
                    if (currentUser == null)
                        return Unauthorized("Current user not found.");

                    if (!currentUser.DepartmentId.HasValue)
                        return BadRequest("Manager dose not hae department");

                    // Manager chỉ được xem user trong cùng phòng ban
                    if (result.DepartmentId != currentUser.DepartmentId)
                        return StatusCode(403, new { Message = "Employee does not exist in this department or has been deleted." });

                    return Ok(ApiResponse<UserDto>.ReturnResult("Get user success", result, 200));
                }

                return Ok(ApiResponse<UserDto>.ReturnResult("Get user success", result, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetUserByIdAsync");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.ToString() });
            }
        }


    }
}
