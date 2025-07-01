using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.CheckinServices;
using EmployeeAPI.Services.ScheduleTimeServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
        /// Lấy thời gian biểu hiện tại, dùng cho api checkin để kiểm tra việc nhân viên đi đúng giờ hay trễ
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetScheduleTime()
        {
            var result = await _scheduleTimeService.GetScheduleTimeAsync();
            return Ok(ApiResponse<ScheduleTime>.ReturnResult("Get Schedule time Success", result, 200));
        }
        /// <summary>
        /// Cập nhật gian biểu, chỉ có admin dc phép dùng
        /// </summary>
        /// <remarks>
        /// { "StartTimeMorning": "08:00:00",
        ///"lateThresholdMinutes": 15,
        ///"EndTimeAfternoon": "17:00:00" }
        /// </remarks>
        [Authorize(Roles = "Administrator")]
        [HttpPut] 
        public async Task<ActionResult<ScheduleTime>> UpdateScheduleTime(ScheduleTime scheduleTime)
        {
            var result = await _scheduleTimeService.UpdateScheduleTimeAsync(scheduleTime);
            return Ok(ApiResponse<ScheduleTime>.ReturnResult("Update Schedule time Success", result, 200));
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
