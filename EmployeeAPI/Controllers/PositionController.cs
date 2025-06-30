using Microsoft.AspNetCore.Mvc;

using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Services.PositionServices;
using Microsoft.AspNetCore.Authorization;
using EmployeeAPI.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.UserService.ResponseModel;
using System.Security.Claims;
using EmployeeAPI.Services.UserService;
using ResponseModel = EmployeeAPI.Services.PositionServices.ResponseModel;
using static EmployeeAPI.Services.PositionServices.ResponseModel;
using EmployeeAPI.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(4)]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;
        private readonly ILogger<PositionController> _logger;
        private readonly IUserService _userService;

        public PositionController(IPositionService positionService, IUserService userService, ILogger<PositionController> logger)
        {
            _positionService = positionService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách chức vụ, manager lấy danh sách theo phòng ban của mình
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAllPositions(string? Search, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _positionService.GetAllAsync(Search, pageIndex, pageSize, currentUserId, currentUserRoles);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.PositionDTO>>.ReturnResult("No result", result, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.PositionDTO>>.ReturnResult("Get list position success", result, 200));
        }

        /// <summary>
        /// Thêm chức vụ trong phỏng ban, manager ko cần thiết nhập department id
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public async Task<IActionResult> AddPosition([FromQuery] ResponseModel.CreatePositionDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            //if (string.IsNullOrWhiteSpace(name)) return BadRequest("Position name cannot be empty");
            var result = await _positionService.AddAsync(dto, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.PositionDTO>.ReturnResult("Create position success", result, 200));
        }

        /// <summary>
        /// cập nhật chức vụ trong phòng ban, chưa authorize
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromQuery] string newName)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _positionService.UpdateAsync(id, newName, currentUserId, currentUserRoles);
            if (result == null)
                return BadRequest(ApiResponse<string>.ReturnResult("Could not find position", null, 404));

            return Ok(ApiResponse<ResponseModel.PositionDTO>.ReturnResult("Update position success", result, 200));
        }

        /// <summary>
        /// Xóa mềm chức vụ trong phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("{positionId}")]
        public async Task<IActionResult> SoftDeletePosition(Guid positionId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = await _positionService.SoftDeleteAsync(positionId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<string>.ReturnResult("Soft delete position success", result, 200));
        }

        /// <summary>
        /// Lấy danh sách nhân viên theo chức vụ, manager chỉ dc lấy danh sách nhân viên theo chứ vụ của phòng ban mình
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeByPosition(Guid PositionId, int? pageSize, int? pageIndex)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _positionService.GetStaffByPositionAsync(PositionId, pageSize, pageIndex, currentUserId, currentUserRoles);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<UserFilterDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<UserFilterDto>>.ReturnResult("Get list employee by position success", pagedResult, 200));
        }

        /// <summary>
        /// Lấy chức vụ theo id, manager chỉ dc lấy chức vụ theo phòng ban của mình    
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("{positionId}")]
        public async Task<IActionResult> GetPositionById(Guid positionId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _positionService.GetByIdAsync(positionId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<PositionDTO>.ReturnResult("Get position success", pagedResult, 200));
        }
    }
}
