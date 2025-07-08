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
        /// Cập nhật thông tin người dùng, sẽ do admin/manager chỉnh sửa
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateStaffAsync([FromForm] ResponseModel.AdminUpdateDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = await _userService.UpdateStaffAsync(dto, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.UserResultDto>.ReturnResult("Update user success", result, 200));
        }

        /// <summary>
        /// Xóa người dùng, sẽ do admin/manager xử lý
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> SoftDeleteAsync(Guid userId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = await _userService.SoftDeleteAsync(userId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<string>.ReturnResult("Delete user success", result, 200));
        }

        /// <summary>
        /// Admin có thể lấy danh sách thông tin của người dùng, manager lấy danh sách theo phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAllUserAsync(string? Search, Guid? positionId, Guid? departmentId, [FromQuery] int? pageIndex = 1, [FromQuery] int? pageSize = 10)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _userService.GetAllAsync(Search, positionId, departmentId, currentUserId, currentUserRoles, pageIndex, pageSize);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<UserResultDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<UserResultDto>>.ReturnResult("Get list user success", pagedResult, 200));
        }

        /// <summary>
        /// Admin có thể lấy thông tin của người dùng, manager chỉ có thể lấy thông tin user theo phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("{userId}")] 
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _userService.GetByIdAsync(userId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<UserResultDto>.ReturnResult("Get user success", result, 200));
        }
    }
}
