using EmployeeAPI.Base;
using EmployeeAPI.Services.HolidayServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;

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
        /// Xem danh sách ngày nghỉ lễ, do admin/manager xử lý
        ///</summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAllHolidays(string? name, int? pageIndex, int? pageSize)
        {
            var pagedResult = await _holidayService.GetAllAsync(name, pageSize, pageIndex);
            if (pagedResult == null || !pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.HolidayResultDto>>.ReturnResult("No result", pagedResult, 200));
            return Ok(ApiResponse<PagedResult<ResponseModel.HolidayResultDto>>.ReturnResult("Get list holiday success", pagedResult, 200));
        }
        /// <summary>
        /// Thêm ngày nghỉ lễ, dùng checkin để kiểm tra xem người dùng có đi làm vào ngày nghỉ ko, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> CreateHoliday(ResponseModel.CreateHolidayDto dto)
        {
            var result = await _holidayService.CreateAsync(dto);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(ApiResponse<ResponseModel.HolidayResultDto>.ReturnResult("Holiday added success", result, 200));
        }
        
        /// <summary>
        /// sủa ngày nghỉ lễ, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public async Task<IActionResult> UpdateHoliday(ResponseModel.UpdateHolidayDto dto)
        {
            var updatedHoliday = await _holidayService.UpdateAsync(dto);
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
            if (holidayId == Guid.Empty)
                return BadRequest();

            var result = await _holidayService.DeleteAsync(holidayId);
            if (result == null)
                return BadRequest();

            return Ok(ApiResponse<string>.ReturnResult("Soft delete holiday success", result, 200));
        }
    }
}
