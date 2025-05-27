using EmployeeAPI.Base;
using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Services.DutyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DutyController : ControllerBase
    {
        private readonly IDutyService _dutyService;
        private readonly ILogger<DutyController> _logger;
        public DutyController(IDutyService dutyService, ILogger<DutyController> logger)
        {
            _dutyService = dutyService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách công việc, chưa authorize
        /// </summary>
        
        [HttpGet]
        public async Task<IActionResult> GetAll(string? SearchTerm, int? pageSize, int? pageIndex)
        {
            try
            {
                var pagedResult = await _dutyService.GetAllAsync(SearchTerm, pageSize, pageIndex);

                /*if (pagedResult.Items.Count() == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot find the result",
                        Data = null,
                        StatusCode = 404
                    });
                }*/

                return Ok(ApiResponse<PagedResult<ResponseModel.DutyDto>>.ReturnResult("Get list duty success", pagedResult, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in GetAll");
                return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the duty id", argEx.Message, 400));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in GetAll");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving duties");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Lấy công việc theo id
        /// </summary>
        [HttpGet("Id")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            try
            {
                var duty = await _dutyService.GetByIdAsync(id);
                if (duty == null)
                {
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the duty id", null, 404));
                }
                return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Get duty by id success", duty, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving duty id");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Thêm công việc, chưa authorize
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public async Task<IActionResult> AddDutyAsync(ResponseModel.CreateDuty dto)
        {
            try
            {
                var result = await _dutyService.AddAsync(dto);
                if (dto == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cannot add duty",
                        Data = null,
                        StatusCode = 404
                    });
                }

                return Ok(ApiResponse<ResponseModel.CreateDuty>.ReturnResult("Create duty success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in AddDutyAsync");
                return StatusCode(400, new { Message = "Add duty failed", Deatail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in AddDutyAsync");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Cập nhật công việc, chưa authorize
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut]
        public async Task<IActionResult> UpdateDutyAsync(ResponseModel.UpdateDuty dto)
        {
            try
            {
                var result = await _dutyService.UpdateAsync(dto);
                if (result == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Database update error", "Invalid input", 400));

                return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Update duty success", result, 200));

            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in AddDutyAsync");
                return StatusCode(400, new { Message = "Update duty failed", Deatail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in AddDutyAsync");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Xóa công việc, chưa authorize
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid id)
        {
            try
            {
                var result = await _dutyService.SoftDeleteAsync(id);
                /*if (result == null)
                {
                    return NotFound();
                }*/
                return Ok(ApiResponse<string>.ReturnResult("Delete duty success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in SoftDeleteAsync");
                return StatusCode(400, new { Message = "Delete duty failed", Deatail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeleteAsync");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find id", StatusCode = 500 });
            }
        }

    }
}
