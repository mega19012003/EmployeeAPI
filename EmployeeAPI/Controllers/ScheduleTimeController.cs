using EmployeeAPI.Models;
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
        public ScheduleTimeController(IScheduleTimeService scheduleTimeService) 
        {
            _scheduleTimeService = scheduleTimeService;
        }

        /// <summary>
        /// Lấy thời gian biểu hiện tại, dùng cho api checkin để kiểm tra việc nhân viên đi đúng giờ hay trễ
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetScheduleTime()
        {
            var result = await _scheduleTimeService.GetScheduleTimeAsync();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// Cập nhật gian biểu 
        /// </summary>
        /// <remarks>
        /// { "startTime": "08:00:00",
        ///"lateThresholdMinutes": 15,
        ///"endTime": "17:00:00" }
        /// </remarks>
        [Authorize(Roles = "Administrator")]
        [HttpPut] 
        public async Task<ActionResult<ScheduleTime>> UpdateScheduleTime(ScheduleTime scheduleTime)
        {
            var result = await _scheduleTimeService.UpdateScheduleTimeAsync(scheduleTime);
            return Ok(result);
        }
    }
}
