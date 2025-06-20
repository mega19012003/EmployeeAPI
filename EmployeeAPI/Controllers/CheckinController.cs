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
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _checkinService.GetAllAsync(StaffName, pageIndex, pageSize, currentUserId, currentUserRoles);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinResultDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.CheckinResultDto>>.ReturnResult("Get list checkin success", pagedResult, 200));
        }

        /// <summary>
        /// lấy checkin
        /// </summary>
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

        /// <summary>
        /// Tạo checkin cho user, , manager chỉ dc checkin hộ nhân viên trong cùng phòng ban
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
        [Authorize]
        [HttpPost("Checkin")]
        public async Task<IActionResult> Create([FromForm] ResponseModel.CreateCheckinDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

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

            var result = await _checkinService.CreateAsync(dto, currentUserId, currentUserRoles);
            if (result == null)
                return BadRequest();

            return Ok(ApiResponse<ResponseModel.CheckinResultDto>.ReturnResult("Checkin create success", result, 200));
        }

        /// <summary>
        /// Cập nhật checkout cho user
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
        [Authorize]
        [HttpPost("Chekout")]
        public async Task<IActionResult> Checkout([FromForm] ResponseModel. CreateCheckoutDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var isAllowed = await _allowedIPService.IsIPAllowedAsync(ip);
            if (!isAllowed)
            {
                return StatusCode(403, new { Message = $"IP address {ip} is not allowed to check out.", Data = ip });
            }

            var result = await _checkinService.CheckoutAsync(dto, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.CheckinResultDto>.ReturnResult("Checkout success", result, 200));
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
        public async Task<IActionResult> Update([FromBody] ResponseModel.UpdateCheckinDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var updated = await _checkinService.UpdateAsync(dto, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.CheckinResultDto>.ReturnResult("", updated, 200));
        }

        /// <summary>
        /// Xóa checkin, nếu thông tin checkin ko có so với sự thật, manager chỉ dc xóa checkin của nhân viên trong cùng phòng ban
        /// </summary>
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
            if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("Cannot find User", result, 200));

            return Ok(ApiResponse<string>.ReturnResult("Delete Checkin Success", result, 200));
        }

        /// <summary>
        /// Lấy danh sách checkin của user, manager chỉ lấy dc danh sách checkin của nhân viên trong cùng phòng ban, employee chỉ dc phép lấy danh sách của chính mình
        /// </summary>
        [Authorize]
        [HttpGet("employee")]
        public async Task<IActionResult> GetCheckinsByUser(Guid userId, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _checkinService.GetCheckinByUserAsync(currentUserId, currentUserRoles, userId, pageIndex, pageSize);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinResultDto>>.ReturnResult("No result", result, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.CheckinResultDto>>.ReturnResult("Get list checkin by user success", result, 200));

        }
    }
}
