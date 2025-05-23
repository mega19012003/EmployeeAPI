using EmployeeAPI.Base;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckinController : ControllerBase
    {
        private readonly ICheckinService _service;
        private readonly ILogger<CheckinController> _logger;

        public CheckinController(ICheckinService service, ILogger<CheckinController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet/*, Authorize*/]
        public async Task<IActionResult> GetAll(string? StaffName, int? pageIndex, int? pageSize)
        {
            try
            {
                var pagedResult = await _service.GetAllAsync(StaffName, pageIndex, pageSize);

                /*if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }*/

                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinDto>>.ReturnResult("Get list checkin success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving checkins");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        [HttpPost/*, Authorize*/]
        public async Task<IActionResult> Create([FromBody] ResponseModel.CreateCheckin dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                if (created == null)
                {
                    return BadRequest();
                }
                /*if (created == null)
                {
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find Staff id", null, 400));
                }*/

                return Ok(ApiResponse<ResponseModel.CheckinDto>.ReturnResult("Checkin create success", created, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while creating a checkin");
                return StatusCode(400, new { Message = "Staff not found", Detail = argEx.Message, StatusCode = 400});
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while creating a checkin");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a checkin");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        [HttpPut/*, Authorize*/]
        public async Task<IActionResult> Update([FromBody] ResponseModel.UpdateCheckin dto)
        {
            try
            {
                //if (!ModelState.IsValid) return BadRequest(ModelState);
                var updated = await _service.UpdateAsync(dto);
                return Ok(ApiResponse<ResponseModel.CheckinDto>.ReturnResult("", updated, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while updating a checkin");
                return StatusCode(400, new { Message = "Checkin not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while updating a checkin");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a checkin");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        [HttpDelete/*, Authorize*/]
        public async Task<IActionResult> SoftDeleteAsync(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("Cannot find Staff id", result, 200));

                return Ok(ApiResponse<string>.ReturnResult("Delete Checkin Success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while deleting a checkin");
                return StatusCode(400, new { Message = "Checkin not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while deleting a checkin");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch
            {
                _logger.LogError("An error occurred while deleting a checkin");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Checkin id", StatusCode = 500 });
            }
        }

        [HttpGet("employee")/*, Authorize*/]
        public async Task<IActionResult> GetCheckinsByStaff(Guid staffId, int? pageIndex, int? pageSize)
        {
            try
            {
                var result = await _service.GetCheckinByStaffAsync(staffId, pageIndex, pageSize);

                /*if (result.Items.Count() == 0)
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });*/

                return Ok(ApiResponse<PagedResult<ResponseModel.CheckinDto>>.ReturnResult("Get list checkin by staff success", result, 200));

                /*if (checkins == null) return NotFound();
                return Ok(checkins);*/
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "An error occurred while retrieving checkins by staff");
                return StatusCode(400, new { Message = "Staff not found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while retrieving checkins by staff");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving checkins by staff");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Staff id", StatusCode = 500 });
            }
        }
    }
}
