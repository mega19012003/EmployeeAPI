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

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetAllPositions(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid user");

                Guid? departmentId = null;

                if (userRole == "Manager")
                {
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null)
                        return Unauthorized("User not found");
                    departmentId = user.DepartmentId;

                }

                var result = await _positionService.GetAllAsync(name, departmentId, pageIndex, pageSize);

                return Ok(ApiResponse<PagedResult<ResponseModel.PositionDTO>>.ReturnResult("Get list position success", result, 200));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetAllPositions controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Thêm chức vụ trong phỏng ban, manager ko cần thiết nhập department id
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public async Task<IActionResult> AddPosition([FromQuery] ResponseModel.CreatePosition dto)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(name)) return BadRequest("Position name cannot be empty");
                var result = await _positionService.AddAsync(dto);
                return Ok(ApiResponse<ResponseModel.PositionDTO>.ReturnResult("Create position success", result, 200));
            }
            catch(ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in AddPosition");
                return StatusCode(400, new { Message = "Add position failed", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in AddPosition");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in AddPosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// cập nhật chức vụ trong phòng ban, chưa authorize
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromQuery] string newName)
        {
            try
            {
                if (id == Guid.Empty || string.IsNullOrWhiteSpace(newName)) 
                    return BadRequest(ApiResponse<string>.ReturnResult("Invalid input", null, 404));

                var result = await _positionService.UpdateAsync(id, newName);
                if (result == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Could not find position", null, 404));

                return Ok(ApiResponse<ResponseModel.UpdatePosition>.ReturnResult("Update position success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in AddPosition");
                return StatusCode(400, new { Message = "Position cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in UpdatePosition");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UpdatePosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Xóa mềm chức vụ trong phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("id")]
        public async Task<IActionResult> SoftDeletePosition([FromQuery] Guid id)
        {
            try
            {
                var result = await _positionService.SoftDeleteAsync(id);
                if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the position id", null, 404));
                return Ok(ApiResponse<string>.ReturnResult("Soft delete position success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in SoftDeletePosition");
                return StatusCode(400, new { Message = "Position cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeletePosition");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SoftDeletePosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Position id", StatusCode = 500 });
            }
        }

        /// <summary>
        /// lấy danh sách nhân viên theo chức vụ, manager chỉ dc lấy danh sách nhân viên theo chứ vụ của phòng ban mình
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeByPosition(Guid PositionId, int? pageSize, int? pageIndex)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid user");

                Guid? departmentId = null;

                if (userRole == "Manager")
                {
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null)
                        return Unauthorized("User not found");

                    departmentId = user.DepartmentId;
                }

                var pagedResult = await _positionService.GetStaffByPositionAsync(departmentId, PositionId, pageSize, pageIndex);
                if (pagedResult == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the Position", null, 404));

                if (!pagedResult.Items.Any())
                    return Ok(ApiResponse<PagedResult<UserFilter>>.ReturnResult("No employees found for this position", pagedResult, 200));

                return Ok(ApiResponse<PagedResult<UserFilter>>.ReturnResult("Get list employee by position success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetEmployeeByPosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Position name", StatusCode = 500 });
            }
        }

    }
}
