using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.CheckinStatusConfigServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogStatusConfigController : ControllerBase
    {
        private readonly ILogStatusConfigService _service;

        public LogStatusConfigController(ILogStatusConfigService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy tất cả cấu hình trạng thái checkin, dùng để tính lương, chỉ có admin dc phép dùng
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _service.GetAllConfigsAsync();
            return Ok(ApiResponse<IEnumerable<LogStatusConfig>>.ReturnResult("Get all configs success", configs, 200));
        }

        /// <summary>
        /// Cập nhật cấu hình, chỉ có admin dc phép dùng
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] LogStatusConfig updated)
        {
            var result = await _service.UpdateConfigAsync(updated);
            return Ok(ApiResponse<LogStatusConfig>.ReturnResult("Update config success", result, 200));
        }
    }
}
