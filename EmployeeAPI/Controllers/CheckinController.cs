using System.Security.Claims;
using Azure.Core;
using EmployeeAPI.Attributes;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
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
    [SwaggerGroupOrder(8)]
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

        // <summary>
        /// Lấy toàn bộ danh sách checkin, manager chỉ dc phép lấy theo phòng ban, employee lấy danh sách của bản thân, admin lấy theo công ty
        // </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Search, Guid? companyId, Guid? departmentId, Guid? positionId, int? Day, int? Month , int? Year, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _checkinService.GetAllAsync(Search, companyId, departmentId, positionId, Day, Month, Year, pageIndex, pageSize, currentUserId, currentUserRoles);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinResultDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.CheckinResultDto>>.ReturnResult("Get list checkin success", pagedResult, 200));
        }

        // <summary>
        /// Lấy checkin
        // </summary>
        [Authorize]
        [HttpGet("{checkinId}")]
        public async Task<IActionResult> GetById(Guid checkinId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _checkinService.GetByIdAsync(checkinId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.CheckinResultDto>.ReturnResult("Get list checkin success", pagedResult, 200));
        }

        // <summary>
        /// Tạo checkin cho user, manager chỉ dc checkin hộ nhân viên trong cùng phòng ban
        // </summary>
        [Authorize]
        [HttpPost("Checkin")]
        public async Task<IActionResult> Checkin([FromForm] ResponseModel.CreateCheckinDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var httpContext = HttpContext;
            var DeviceInfo = httpContext?.Request?.Form["DeviceInfo"].ToString();

            // Lấy IP client
            var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var isAllowed = await _allowedIPService.IsIPAllowedAsync(ip);
            if (!isAllowed)
            {
                return StatusCode(403, new { Message = $"IP address {ip} is not allowed to check in.", Data = ip });
            }

            if (dto.userId == null || dto.userId == Guid.Empty)
            {
                dto.userId = currentUserId;
            }

            var result = await _checkinService.CheckinAsync(dto.userId, DeviceInfo, ip, dto.Note, currentUserId, currentUserRoles);
            if (result == null)
                return BadRequest();

            return Ok(ApiResponse<ResponseModel.CheckinResultDto>.ReturnResult("Checkin Morning success", result, 200));
        }

        // <summary>
        /// Cập nhật checkout cho user
        // </summary>
        [Authorize]
        [HttpPut("Chekout")]
        public async Task<IActionResult> Chekout([FromForm] ResponseModel.CreateCheckoutDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var httpContext = HttpContext;
            var DeviceInfo = httpContext?.Request?.Form["DeviceInfo"].ToString();

            var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var isAllowed = await _allowedIPService.IsIPAllowedAsync(ip);
            if (!isAllowed)
            {
                return StatusCode(403, new { Message = $"IP Không hợp lệ để checkin", Data = $"IP ({ip}) không nằm trong khoảng cho phép để checkin", StatusCode = 403 });
            }

            var result = await _checkinService.CheckoutAsync(dto.userId, DeviceInfo, ip, dto.Note, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.CheckinResultDto>.ReturnResult("Checkout success", result, 200));
        }

        // <summary>
        /// Cập nhật thông tin checkin, nếu thông tin bị sai hoặc nhân viên hoặc lách luật, manager chỉ dc update checkin của nhân viên trong cùng phòng ban
        // </summary>
        /// <remarks>
        /// - None = 0 (Chưa checkin/checkout)
        /// - OnTime = 1 (Đúng giờ)
        /// - Late = 2 (Đi trễ)
        /// - LeaveEarly = 3 (Về sớm)
        /// - LateAndLeaveEarly = 4 (Đi trễ và về sớm)
        /// - Overtime = 5 (Làm thêm giờ)
        /// - LateAndOvertime = 6 (Đi trễ và làm thêm giờ)
        /// - Absent = 7 (Vắng)
        /// - OnHoliday = 8 (Làm vào ngày nghỉ)
        /// - OnHolidayLate = 9 (Đi trễ vào ngày nghỉ)
        /// - OnHolidayLeaveEarly = 10 (Về sớm vào ngày nghỉ)
        /// - OnHolidayOvertime = 11 (Làm thêm giờ vào ngày nghỉ)
        /// - OnHolidayLateAndOvertime = 12 (Đi trễ và làm thêm giờ vào ngày nghỉ)
        /// - OnHolidayLateAndLeaveEarly = 13 (Đi trễ và về sớm vào ngày lễ)
        /// - Others = 14 (khác)
        /// </remarks>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ResponseModel.UpdateCheckinDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var updated = await _checkinService.UpdateAsync(dto, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.CheckinResultDto>.ReturnResult("Update success", updated, 200));
        }

        // <summary>
        /// Xóa checkin, nếu thông tin checkin ko có so với sự thật, manager chỉ dc xóa checkin của nhân viên trong cùng phòng ban
        // </summary>
        [Authorize(Roles = "Administrator,Manager")]
        [HttpDelete("{checkinId}")]
        public async Task<IActionResult> SoftDeleteAsync(Guid checkinId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _checkinService.DeleteAsync(checkinId, currentUserId, currentUserRoles);
            //var result = await _checkinService.DeleteAsync(id);
            if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("Không tìm thấy user", result, 200));

            return Ok(ApiResponse<string>.ReturnResult("Delete Checkin Success", result, 200));
        }

        [Authorize]
        [HttpGet("users-checkins")]
        public async Task<IActionResult> GetAllUsersCheckins(string? Search, Guid? companyId, Guid? departmentId, Guid? positionId, int? day, int? month, int? year, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return StatusCode(500, new { Message = "Internal server error", Detail = "Invalid user ID", StatusCode = 500 });

            var currentRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _checkinService.GetUsersWithCheckinsAsync(Search, companyId, departmentId, positionId, day, month, year, pageIndex, pageSize, currentUserId, currentRoles);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.UserWithCheckinsDto>>.ReturnResult("No data found", result, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.UserWithCheckinsDto>>.ReturnResult("Get checkin list success", result, 200));
        }

    }
}
