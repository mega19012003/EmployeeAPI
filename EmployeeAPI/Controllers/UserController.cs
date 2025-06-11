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
using EmployeeAPI.Attributes;
using EmployeeAPI.Models;
using System.Text.RegularExpressions;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(2)]
    public class UserController : ControllerBase
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
        [HttpPut("AdminUpdateUser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AdminUpdateStaffAsync([FromForm] ResponseModel.AdminUpdateDto dto)
        {
            /* var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             var userRole = User.FindFirst(ClaimTypes.Role)?.Value;*/
            var result = await _userService.AdminUpdateStaffAsync(dto, User);

            return Ok(ApiResponse<ResponseModel.UserDto>.ReturnResult("Update user success", result, 200));
        }

        /// <summary>
        /// Cập nhật thông tin người dùng, sẽ do manager chỉnh, manager ko dc chỉnh role và departmentid sẽ tự gán cho user
        /// </summary>
        [Authorize(Roles = "Manager")]
        [HttpPut("ManagerUpdateUser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ManagerUpdateStaffAsync([FromForm] ResponseModel.ManagerUpdateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _userService.ManagerUpdateStaffAsync(dto, managerId, User);

            return Ok(ApiResponse<ResponseModel.UserDto>.ReturnResult("Update staff success", result, 200));
        }

        /// <summary>
        /// Xóa người dùng, sẽ do admin/manager xử lý
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("id")]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid Id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var isManager = User.IsInRole("Manager");
            var isAdmin = User.IsInRole("Administrator");

            if (isManager && !isAdmin)
            {
                var currentUser = await _userService.GetByIdAsync(currentUserId);
                if (currentUser == null)
                    return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

                if (!currentUser.DepartmentId.HasValue)
                    return StatusCode(400, new { Message = "Delete user failed", Detail = "Manager chưa có phòng ban", StatusCode = 400 });

                var findUser = await _userService.GetByIdAsync(Id);
                if (findUser.DepartmentId != currentUser.DepartmentId)
                    return StatusCode(400, new { Message = "Delete user failed", Detail = "Manager cannot delete user from other department", StatusCode = 400 });
            }

            var result = await _userService.SoftDeleteAsync(Id, User);

            return Ok(ApiResponse<string>.ReturnResult("Delete user success", result, 200));
        }

        /// <summary>
        /// Admin có thể lấy danh sách thông tin của người dùng, manager lấy danh sách theo phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAllUserAsync(string? Name, Guid? departmentId, int? pageSize, int? pageIndex)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var isManager = User.IsInRole("Manager");
            var isAdmin = User.IsInRole("Administrator");

            if (isManager && !isAdmin)
            {
                var result = await _userService.GetByIdAsync(currentUserId);
                if (result == null)
                    return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

                if (!result.DepartmentId.HasValue)
                    return StatusCode(400, new { Message = "Delete user failed", Detail = "Manager chưa có phòng ban", StatusCode = 400 });

                departmentId = result.DepartmentId;
            }

            var pagedResult = await _userService.GetAllAsync(Name, departmentId, pageIndex, pageSize);
            if (pagedResult == null)
                return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the department", null, 404));

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<UserDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<UserDto>>.ReturnResult("Get list user success", pagedResult, 200));
        }

        /// <summary>
        /// Admin có thể lấy thông tin của người dùng, manager chỉ có thể lấy thông tin user theo phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("id")] 
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var isManager = User.IsInRole("Manager");
            var isAdmin = User.IsInRole("Administrator");

            var result = await _userService.GetByIdAsync(id);
            if (result == null)
                return StatusCode(400, new { Message = "Get user failed", Detail = "User not found", StatusCode = 400 });


            if (isManager)
            {
                var currentUser = await _userService.GetByIdAsync(currentUserId);
                if (currentUser == null)
                    return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

                if (!currentUser.DepartmentId.HasValue)
                    return StatusCode(400, new { Message = "Get user failed", Detail = "Manager does not have department", StatusCode = 400 });

                // Manager chỉ được xem user trong cùng phòng ban
                if (result.DepartmentId != currentUser.DepartmentId)
                    return StatusCode(403, new { Message = "Get user failed", Detail = "User does not exist in this department or has been deleted.", StatusCode = 403 });

                return Ok(ApiResponse<UserDto>.ReturnResult("Get user success", result, 200));
            }

            return Ok(ApiResponse<UserDto>.ReturnResult("Get user success", result, 200));
        }
    }
}
