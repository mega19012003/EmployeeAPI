using System.Security.Claims;
using EmployeeAPI.Attributes;
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
    [SwaggerGroupOrder(5)]
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
        /// Lấy danh sách công việc manager chỉ lấy dc công việc do mình tạo ra, employee chỉ lấy dc công việc có bản thân ở trong
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? name, int? pageSize, int? pageIndex)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _dutyService.GetAllAsync(currentUserId, currentUserRoles, name, pageIndex, pageSize);
            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.DutyDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.DutyDto>>.ReturnResult("Get list duty success", pagedResult, 200));
        }

        /// <summary>
        /// Lấy công việc theo id
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("Id")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var duty = await _dutyService.GetByIdAsync(id, currentUserId, currentUserRoles);
                
                return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Get duty success", duty, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving duty");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Thêm công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public async Task<IActionResult> AddDutyAsync(ResponseModel.CreateDuty dto)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _dutyService.AddDutyAsync(dto, currentUserId, currentUserRoles);

                return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Create duty success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in AddDutyAsync");
                return StatusCode(400, new { Message = "Add duty failed", Detail = argEx.Message, StatusCode = 400 });
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
        /// Thêm chi tiết công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost("DutyDetail")]
        public async Task<IActionResult> AddDutyDetailAsync(ResponseModel.GetDutyDto dto)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _dutyService.AddDutyDetailAsync(dto, dto.Id, currentUserId, currentUserRoles);

                return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Create duty detail success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in AddDutyAsync");
                return StatusCode(400, new { Message = "Add duty detail failed", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in AddDutyAsync");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty detail");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Cập nhật công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut]
        public async Task<IActionResult> UpdateDutyAsync(ResponseModel.UpdateDuty dto)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _dutyService.UpdateDutyAsync(dto, currentUserId, currentUserRoles);

                if (result == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Database update error", "Invalid input", 400));

                return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Update duty success", result, 200));

            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in AddDutyAsync");
                return StatusCode(400, new { Message = "Update duty failed", Detail = argEx.Message, StatusCode = 400 });
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
        /// Cập nhật chi tiết công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut("DutyDetail")]
        public async Task<IActionResult> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetail dto)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var result = await _dutyService.UpdateDutyDetailAsync(dto, currentUserId, currentUserRoles);

                if (result == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Database update error", "Invalid input", 400));

                return Ok(ApiResponse<ResponseModel.DutyDetailDto>.ReturnResult("Update duty success", result, 200));

            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in AddDutyAsync");
                return StatusCode(400, new { Message = "Update duty failed", Detail = argEx.Message, StatusCode = 400 });
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
        /// Xóa công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("id")]
        public async Task<IActionResult> SoftDeleteDutyAsync([FromForm] Guid id)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
                var result = await _dutyService.SoftDeleteDutyAsync(id, currentUserId, currentUserRoles);
                return Ok(ApiResponse<string>.ReturnResult("Delete duty success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in SoftDeleteAsync");
                return StatusCode(400, new { Message = "Delete duty failed", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeleteAsync");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
        /// <summary>
        /// Xóa chi tiết công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("DutyDetail")]
        public async Task<IActionResult> SoftDeleteDutyDetailAsync([FromForm] Guid id)
        {
            try
            {
                var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                    return Unauthorized("UserId invalid");

                var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
                var result = await _dutyService.SoftDeleteDutyDetailAsync(id, currentUserId, currentUserRoles);
                return Ok(ApiResponse<string>.ReturnResult("Delete duty success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in SoftDeleteAsync");
                return StatusCode(400, new { Message = "Delete duty failed", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeleteAsync");
                return BadRequest(ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Duty");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
    }
}
