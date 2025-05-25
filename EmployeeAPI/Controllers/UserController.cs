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
        /// Cập nhật thông tin người dùng, sẽ do admin chỉnh sửa hết thông tin, chưa authorize
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
        /// Cập nhật thông tin người dùng, sẽ do manager chỉnh, manager ko dc chỉnh role và departmentid sẽ tự gán cho staff, chưa authorize
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
        /// Xóa người dùng, sẽ do admin/manager xử lý, chưa authorize
        /// </summary>
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
        /// Lấy toàn bộ thông tin người dùng, chưa authorize
        /// </summary>
        [HttpGet("GetAll"), /*, Authorize*/]
        public async Task<IActionResult> GetAllUserAsync(string? Name, Guid? departmentId, int? pageSize, int? pageIndex)
        {
            try
            {
                var pagedResult = await _userService.GetAllAsync(Name, departmentId, pageSize, pageIndex);
                if (pagedResult == null)
                {
                    return BadRequest();
                }

                return Ok(ApiResponse<PagedResult<UserDto>>.ReturnResult("Get list user success", pagedResult, 200));
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
    }
}
