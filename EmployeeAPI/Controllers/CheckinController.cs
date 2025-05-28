using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckinController : ControllerBase
    {
        private readonly ICheckinService _service;
        private readonly ILogger<CheckinController> _logger;

        public CheckinController(ICheckinService service, ILogger<CheckinController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// lấy toàn bộ danh sách checkin, chỉ có admin dc phép dùng
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? StaffName, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _service.GetAllAsync(StaffName, pageIndex, pageSize);

                /*if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }*/

                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinDto>>.ReturnResult("Get list checkin success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving checkins");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Tạo checkin cho user
        /// </summary>
        ///<remarks>
        /// Nhập dateTime theo dạng "yyyy-MM-ddTHH:mm:ss" (ví dụ: "2000-01-01T08:00:00" )
        /// </remarks>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResponseModel.CreateCheckin dto)
        {
            try
            {
                // Lấy userId từ Claims (ngầm định user đã đăng nhập)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { Message = "User ID not found in token." });
                }

                dto.userId = userId;

                var created = await _service.CreateAsync(dto);
                if (created == null)
                {
                    return BadRequest();
                }
                /*if (created == null)
                {
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find Staff id", null, 400));
                }*/

                return Ok(ApiResponse<ResponseModel.CheckinDto>.ReturnResult("Checkin create success", created, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while creating a checkin");
                return StatusCode(400, new { Message = "Checkin Failed", Detail = argEx.Message, StatusCode = 400});
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while creating a checkin");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a checkin");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Cập nhật thông tin checkin, nếu thông tin bị sai hoặc nhân viên, nghỉ có phép hoặc lách luật, manager chỉ dc update checkin của nhân viên trong cùng phòng ban
        /// </summary>
        /// <remarks>
        /// - CheckinStatus enum values:
        /// - OnTime = 0 (đúng giờ)
        /// - Late = 1 (Đi trễ hơn 15 phút)
        /// - LeaveEarly = 2 (Về sớm)
        /// - Overtime = 3 (làm tăng ca)
        /// - Absent = 4 (Vắng)
        /// - LeaveWithPermission = 5 (Vắng có phép)
        /// - Others = 6 (lí do khác)
        /// </remarks>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ResponseModel.UpdateCheckin dto)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var updated = await _service.UpdateAsync(dto, currentUserId, currentUserRoles);
                return Ok(ApiResponse<ResponseModel.CheckinDto>.ReturnResult("", updated, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while updating a checkin");
                return StatusCode(400, new { Message = "Checkin not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while updating a checkin");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a checkin");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Xóa checkin, nếu thông tin checkin ko có so với sự thật, manager chỉ dc xóa checkin của nhân viên trong cùng phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpDelete]
        public async Task<IActionResult> SoftDeleteAsync(Guid id)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _service.DeleteAsync(id, currentUserId, currentUserRoles);
                //var result = await _service.DeleteAsync(id);
                if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("Cannot find Staff id", result, 200));

                return Ok(ApiResponse<string>.ReturnResult("Delete Checkin Success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while deleting a checkin");
                return StatusCode(400, new { Message = "Checkin not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                _logger.LogWarning(unAuthEx, "Unauthorized access while deleting checkin");
                return StatusCode(403, new { Message = "Access Denied", Detail = unAuthEx.Message, StatusCode = 403 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while deleting a checkin");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch
            {
                _logger.LogError("An error occurred while deleting a checkin");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Checkin id", StatusCode = 500 });
            }
        }

        /// <summary>
        /// Lấy danh sách checkin của user, manager chỉ lấy dc danh sách checkin của nhân viên trong cùng phòng ban, employee chỉ dc phép lấy danh sách của chính mình
        /// </summary>
        [Authorize]
        [HttpGet("employee")]
        public async Task<IActionResult> GetCheckinsByUser(Guid staffId, int? pageIndex, int? pageSize)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _service.GetCheckinByUserAsync(currentUserId, currentUserRoles, staffId, pageIndex, pageSize);
                //var result = await _service.GetCheckinByUserAsync(staffId, pageIndex, pageSize);
                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinDto>>.ReturnResult("Get list checkin by staff success", result, 200));

            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while retrieving checkins by staff");
                return StatusCode(400, new { Message = "Staff not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while retrieving checkins by staff");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving checkins by staff");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Staff id", StatusCode = 500 });
            }
        }
    }
}
