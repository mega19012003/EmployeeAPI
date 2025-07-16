using CloudinaryDotNet;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.CheckinServices;
using EmployeeAPI.Services.ScheduleTimeServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.Design;
using System.Security.Claims;
using static EmployeeAPI.Services.ScheduleTimeServices.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleTimeController : ControllerBase
    {
        private readonly IScheduleTimeService _scheduleTimeService;
        private readonly ICheckinService _checkinService;
        public ScheduleTimeController(IScheduleTimeService scheduleTimeService, ICheckinService checkinService)
        {
            _checkinService = checkinService;
            _scheduleTimeService = scheduleTimeService;
        }


        /// <summary>
        /// Lấy toàn bộ schedule time, chỉ system Admin mới dc dùng
        /// </summary>
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAllScheduleTime(Guid? companyId, int? pageIndex, int? pageSize)
        {
            //var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            //    return Unauthorized("UserId invalid");
            //var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _scheduleTimeService.GetAllAsync(companyId, pageIndex, pageSize/*, currentUserId, currentUserRoles*/);
            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ScheduleDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ScheduleDto>>.ReturnResult("Get Schedule time Success", pagedResult, 200));
        }

        /// <summary>
        /// Lấy thời gian biểu hiện tại, bổ hợ cho api checkin để kiểm tra việc nhân viên đi đúng giờ hay trễ, chỉ systeam admin lấy được theo toàn bộ id
        /// </summary>
        [Authorize]
        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetScheduleByIdTime(Guid scheduleId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _scheduleTimeService.GetScheduleTimeByIdAsync(scheduleId, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ScheduleDto>.ReturnResult("Get Schedule time Success", result, 200));
        }
        /// <summary>
        /// Cập nhật gian biểu, chỉ có admin dc phép dùng
        /// </summary>
        /// <remarks>
        /// { "StartTimeMorning": "08:00:00",
        /// "EndTimeMorning": "12:00:00",
        /// "lateThresholdMinutes": 5,
        /// "StartTimeMorning": "13:00:00",
        ///"EndTimeAfternoon": "17:00:00" }
        /// </remarks>
        [Authorize(Roles = "Administrator")]
        [HttpPut] 
        public async Task<ActionResult<ScheduleTime>> UpdateScheduleTime(ScheduleTime scheduleTime)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _scheduleTimeService.UpdateScheduleTimeAsync(scheduleTime, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ScheduleDto>.ReturnResult("Update Schedule time Success", result, 200));
        }

        /// <summary>
        /// Test code Tự động chấm công
        /// </summary>
        /*[HttpGet("test-absent")]
        public async Task<IActionResult> TestAbsent()
        {
            var schedule = await _scheduleTimeService.GetScheduleTimeAsync();
            if (schedule == null)
                return NotFound("Không có ca làm việc");

            await _checkinService.AutoMarkAbsentAsync(schedule.EndTimeAfternoon);
            return Ok("Đã thử đánh dấu absent");
        }*/
    }
}
