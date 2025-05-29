using EmployeeAPI.Models;
using EmployeeAPI.Services.CheckinStatusConfigServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckinStatusConfigController : ControllerBase
    {
        private readonly ICheckinStatusConfigService _service;

        public CheckinStatusConfigController(ICheckinStatusConfigService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy tất cả cấu hình trạng thái checkin, chỉ có admin dc phép dùng
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _service.GetAllConfigsAsync();
            return Ok(configs);
        }

        /// <summary>
        /// Cập nhật cấu hình, chỉ có admin dc phép dùng
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] CheckinStatusConfig updated)
        {
            if (id != updated.Id)
                return BadRequest("ID mismatch");

            try
            {
                await _service.UpdateConfigAsync(updated);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
