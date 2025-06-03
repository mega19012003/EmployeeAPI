using System.Security.Claims;
using Azure.Core;
using EmployeeAPI.Attributes;
using EmployeeAPI.Base;
using EmployeeAPI.Services.AllowedIpServices;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(6)]
    public class CheckinController : ControllerBase
    {
        private readonly ICheckinService _checkinService;
        private readonly ILogger<CheckinController> _logger;
        private readonly IAllowedIPService _allowedIPService;

        public CheckinController(ICheckinService checkinService, ILogger<CheckinController> logger, IAllowedIPService allowedIPService)
        {
            _allowedIPService = allowedIPService;
            _checkinService = checkinService;
            _logger = logger;
        }

        /// <summary>
        /// lấy toàn bộ danh sách checkin, manager chỉ dc phép lấy theo phòng ban
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? StaffName, int? pageIndex, int? pageSize)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var pagedResult = await _checkinService.GetAllAsync(StaffName, pageIndex, pageSize, currentUserId, currentUserRoles);

                if (!pagedResult.Items.Any())
                    return Ok(ApiResponse<PagedResult<ResponseModel.CheckinDto>>.ReturnResult("No result", pagedResult, 200));

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
        /// Nhập dateTime theo dạng "yyyy-MM-ddTHH:mm:ss" (ví dụ: "2000-01-01T08:00:00" ) hoặc excute luôn cũng dc
        /// </remarks>
        [Authorize]
        [HttpPost("Checkin")]
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

                // Lấy IP client từ HttpContext
                //var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

                var isAllowed = await _allowedIPService.IsIPAllowedAsync(ip);
                if (!isAllowed)
                {
                    return StatusCode(403, new { Message = $"IP address {ip} is not allowed to check in.", Data = ip });
                }
                //dto.IpAddress = ip ?? "Unknown";

                var created = await _checkinService.CreateAsync(dto);
                if (created == null)
                {
                    return BadRequest();
                }

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
        /// Cập nhật checkout cho user
        /// </summary>
        ///<remarks>
        /// Nhập dateTime theo dạng "yyyy-MM-ddTHH:mm:ss" (ví dụ: "2000-01-01T08:00:00" ) hoặc excute luôn cũng dc
        /// </remarks>
        [Authorize]
        [HttpPost("Chekout")]
        public async Task<IActionResult> Checkout([FromBody] ResponseModel.CreateCheckout dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { Message = "User ID not found in token." });
                }
                dto.userId = userId;
                var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                var isAllowed = await _allowedIPService.IsIPAllowedAsync(ip);
                if (!isAllowed)
                {
                    return StatusCode(403, new { Message = $"IP address {ip} is not allowed to check out.", Data = ip });
                }

                var result = await _checkinService.CheckoutAsync(dto);
                return Ok(ApiResponse<ResponseModel.CheckinDto>.ReturnResult("Checkout success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while updating a checkout");
                return StatusCode(400, new { Message = "Checkin Failed", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while updating a checkout");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a checkout");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }


        /// <summary>
        /// Cập nhật thông tin checkin, nếu thông tin bị sai hoặc nhân viên, nghỉ có phép hoặc lách luật, manager chỉ dc update checkin của nhân viên trong cùng phòng ban
        /// </summary>
        /// <remarks>
        /// - CheckIn Status enum values:
        /// - OnTime = 0 (đúng giờ)
        /// - Late = 1 (Đi trễ hơn 15 phút)
        /// - Absent = 4 (Vắng)
        /// - LeaveWithPermission = 5 (Vắng có phép)
        /// - Others = 6 (lí do khác)
        /// -
        /// - CheckOut Status enum values:
        /// - OnTime = 0 (đúng giờ)
        /// - LeaveEarly = 2 (Về sớm so với quy định)
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

                var updated = await _checkinService.UpdateAsync(dto, currentUserId, currentUserRoles);
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
        [HttpDelete("id")]
        public async Task<IActionResult> SoftDeleteAsync(Guid id)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _checkinService.DeleteAsync(id, currentUserId, currentUserRoles);
                //var result = await _checkinService.DeleteAsync(id);
                if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("Cannot find User", result, 200));

                return Ok(ApiResponse<string>.ReturnResult("Delete Checkin Success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while deleting a checkin");
                return StatusCode(400, new { Message = "Checkin not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while deleting a checkin");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while deleting a checkin");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
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

                var result = await _checkinService.GetCheckinByUserAsync(currentUserId, currentUserRoles, staffId, pageIndex, pageSize);

                if (!result.Items.Any())
                    return Ok(ApiResponse<PagedResult<ResponseModel.CheckinDto>>.ReturnResult("No result", result, 200));

                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinDto>>.ReturnResult("Get list checkin by user success", result, 200));

            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while retrieving checkins by user");
                return StatusCode(400, new { Message = "User not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while retrieving checkins by user");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving checkins by user");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
    }
}
