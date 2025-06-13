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
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var duty = await _dutyService.GetByIdAsync(id, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Get duty success", duty, 200));
        }

        /// <summary>
        /// Thêm công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public async Task<IActionResult> AddDutyAsync(ResponseModel.CreateDuty dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.AddDutyAsync(dto, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Create duty success", result, 200));
        }

        /// <summary>
        /// Thêm chi tiết công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost("DutyDetail")]
        public async Task<IActionResult> AddDutyDetailAsync(ResponseModel.GetDutyDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.AddDutyDetailAsync(dto, dto.Id, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Create duty detail success", result, 200));
        }

        /// <summary>
        /// Cập nhật công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut]
        public async Task<IActionResult> UpdateDutyAsync(ResponseModel.UpdateDuty dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.UpdateDutyAsync(dto, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyDto>.ReturnResult("Update duty success", result, 200));
        }

        /// <summary>
        /// Cập nhật chi tiết công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut("DutyDetail")]
        public async Task<IActionResult> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetail dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.UpdateDutyDetailAsync(dto, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyDetailDto>.ReturnResult("Update duty success", result, 200));
        }

        /// <summary>
        /// Xóa công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("id")]
        public async Task<IActionResult> SoftDeleteDutyAsync([FromForm] Guid id)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = await _dutyService.SoftDeleteDutyAsync(id, currentUserId, currentUserRoles);
            return Ok(ApiResponse<string>.ReturnResult("Delete duty success", result, 200));
        }
        /// <summary>
        /// Xóa chi tiết công việc
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("DutyDetail")]
        public async Task<IActionResult> SoftDeleteDutyDetailAsync([FromForm] Guid id)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = await _dutyService.SoftDeleteDutyDetailAsync(id, currentUserId, currentUserRoles);
            return Ok(ApiResponse<string>.ReturnResult("Delete duty success", result, 200));
        }
    }
}
