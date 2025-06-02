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
        [Authorize(Roles = "Aministrator")]
        [HttpGet]
        public async Task<IActionResult> GetAllHolidays(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _holidayService.GetAllAsync(name, pageSize, pageIndex);
                if (pagedResult == null || !pagedResult.Items.Any())
                    return Ok(ApiResponse<PagedResult<ResponseModel.HolidayDto>>.ReturnResult("No result", pagedResult, 200));
                return Ok(ApiResponse<PagedResult<ResponseModel.HolidayDto>>.ReturnResult("Get list holiday success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving holidays");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
        /// <summary>
        /// Thêm ngày nghỉ lễ, dùng checkin để kiểm tra xem người dùng có đi làm vào ngày nghỉ ko, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Aministrator")]
        [HttpPost]
        public async Task<IActionResult> CreateHoliday(ResponseModel.CreateHoliday dto)
        {
            try
            {
               
                var result = await _holidayService.CreateAsync(dto);
                if (result == null)
                {
                    return BadRequest();
                }
                return Ok(ApiResponse<ResponseModel.HolidayDto>.ReturnResult("Holiday added success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while adding the holiday");
                return StatusCode(400, new { Message = "Holiday cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while adding the holiday");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the holiday");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
        
        /// <summary>
        /// sủa ngày nghỉ lễ, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Aministrator")]
        [HttpPut]
        public async Task<IActionResult> UpdateHoliday(ResponseModel.UpdateHoliday dto)
        {
            try
            {
                var updatedHoliday = await _holidayService.UpdateAsync(dto);
                if (updatedHoliday == null)
                    return BadRequest();

                return Ok(ApiResponse<ResponseModel.HolidayDto>.ReturnResult("Update holiday success", updatedHoliday, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while updating the holiday");
                return StatusCode(400, new { Message = "Holiday cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while updating the holiday");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the holiday");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }


        /// <summary>
        /// Xóa ngày nghỉ lễ, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Aministrator")]
        [HttpDelete]
        public async Task<IActionResult> SoftDeleteHoliday(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest();

                var result = await _holidayService.DeleteAsync(id);
                if (result == null)
                    return BadRequest();

                return Ok(ApiResponse<string>.ReturnResult("Soft delete holiday success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while deleting the holiday");
                return StatusCode(400, new { Message = "Holiday cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while deleting the holiday");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while soft deleting the holiday");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
    }
}
