using EmployeeAPI.Base;
using EmployeeAPI.Services.HolidayServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _holidayService;
        private readonly ILogger<HolidayController> _logger;
        public HolidayController(IHolidayService holidayService, ILogger<HolidayController> logger)
        {
            _holidayService = holidayService;
            _logger = logger;
        }
        /// <summary>
        /// Xem danh sách ngày nghỉ lễ, systeamAdmin xem đươc toàn bộ cấu hình
        ///</summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllHolidays(string? Search, Guid companyId, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _holidayService.GetAllAsync(Search, companyId, pageSize, pageIndex, currentUserId, currentUserRoles);
            if (pagedResult == null || !pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.HolidayResultDto>>.ReturnResult("No result", pagedResult, 200));
            return Ok(ApiResponse<PagedResult<ResponseModel.HolidayResultDto>>.ReturnResult("Get list holiday success", pagedResult, 200));
        }

        /// <summary>
        /// Xem ngày nghỉ lễ theo id, do admin/systemAdmin 
        ///</summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet("{HolidayId}")]
        public async Task<IActionResult> GetHolidayById(Guid HolidayId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _holidayService.GetByIdAsync(HolidayId, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.HolidayResultDto>.ReturnResult("Get holiday success", pagedResult, 200));
        }

        /// <summary>
        /// Thêm ngày nghỉ lễ, dùng checkin để kiểm tra xem người dùng có đi làm vào ngày nghỉ ko, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> CreateHoliday(ResponseModel.CreateHolidayDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _holidayService.CreateAsync(dto, currentUserId, currentUserRoles);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(ApiResponse<ResponseModel.HolidayResultDto>.ReturnResult("Holiday added success", result, 200));
        }
        
        /// <summary>
        /// Cập nhật ngày nghỉ lễ, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public async Task<IActionResult> UpdateHoliday(ResponseModel.UpdateHolidayDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var updatedHoliday = await _holidayService.UpdateAsync(dto, currentUserId, currentUserRoles);
            if (updatedHoliday == null)
                return BadRequest();

            return Ok(ApiResponse<ResponseModel.HolidayResultDto>.ReturnResult("Update holiday success", updatedHoliday, 200));
        }


        /// <summary>
        /// Xóa ngày nghỉ lễ, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{holidayId}")]
        public async Task<IActionResult> SoftDeleteHoliday(Guid holidayId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _holidayService.DeleteAsync(holidayId, currentUserId, currentUserRoles);
            return Ok(ApiResponse<string>.ReturnResult("Soft delete holiday success", result, 200));
        }
    }
}
