using System.Security.Claims;
using EmployeeAPI.Attributes;
using EmployeeAPI.Base;
using EmployeeAPI.Services.DutyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(7)]
    public class DutyController : ControllerBase
    {
        private readonly IDutyService _dutyService;
        private readonly ILogger<DutyController> _logger;
        public DutyController(IDutyService dutyService, ILogger<DutyController> logger)
        {
            _dutyService = dutyService;
            _logger = logger;
        }

        // <summary>
        /// Admin lấy danh sách công việc cảu công ty, manager chỉ lấy dc công việc do mình tạo ra, employee chỉ lấy dc công việc có bản thân ở trong
        // </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Search, Guid? companyId, int? day, int? month, int? year, int? pageSize, int? pageIndex)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _dutyService.GetAllAsync(currentUserId, currentUserRoles, Search, companyId, day, month, year, pageIndex, pageSize);
            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.DutyResultDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.DutyResultDto>>.ReturnResult("Get list duty success", pagedResult, 200));
        }

        // <summary>
        /// Lấy công việc theo id
        // </summary>
        [Authorize]
        [HttpGet("{dutyId}")]
        public async Task<IActionResult> GetDutyByIdAsync(Guid dutyId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var duty = await _dutyService.GetDutyByIdAsync(dutyId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyResultDto>.ReturnResult("Get duty success", duty, 200));
        }

        // <summary>
        /// Lấy chi tiết công việc theo id, ADmin lấy chi tiết theo công ty, manager lấy thoe phòng ban, employee lấy chit tiết có bản thân ở trong
        // </summary>
        [Authorize]
        [HttpGet("detail/{detailId}")]
        public async Task<IActionResult> GetDetailByIdAsync(Guid detailId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var duty = await _dutyService.GetDutyDetailByIdAsync(detailId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyDetailResultDto>.ReturnResult("Get duty success", duty, 200));
        }

        // <summary>
        /// Thêm công việc, do admin/manager xử lý 
        // </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public async Task<IActionResult> AddDutyAsync(ResponseModel.CreateDutyDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.AddDutyAsync(dto, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyResultDto>.ReturnResult("Create duty success", result, 200));
        }

        // <summary>
        /// Thêm chi tiết công việc, do admin/manager xử lý 
        // </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost("DutyDetail")]
        public async Task<IActionResult> AddDutyDetailAsync(ResponseModel.GetDutyDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.AddDutyDetailAsync(dto, dto.Id, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyResultDto>.ReturnResult("Create duty detail success", result, 200));
        }

        // <summary>
        /// Cập nhật công việc, do admin/manager xử lý 
        // </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut]
        public async Task<IActionResult> UpdateDutyAsync(ResponseModel.UpdateDutyDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.UpdateDutyAsync(dto, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyResultDto>.ReturnResult("Update duty success", result, 200));
        }

        // <summary>
        /// Đánh dấu là hoàn tất công việc, manager có thể đánh dấu công việc của mình
        // </summary>
        //[Authorize(Roles = "Administrator, Manager")]
        //[HttpPut("{dutyId}")]
        //public async Task<IActionResult> MarkDutyAsCompletedAsync(Guid dutyId)
        //{
        //    var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
        //        return Unauthorized("UserId invalid");

        //    var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        //    var result = await _dutyService.MarkDutyAsCompletedAsync(dutyId, currentUserId, currentUserRoles);

        //    return Ok(ApiResponse<ResponseModel.DutyResultDto>.ReturnResult("Update duty success", result, 200));
        //}

        // <summary>
        /// Cập nhật chi tiết công việc, do admin/manager xử lý 
        // </summary>
        [Authorize(Roles = "Administrator, Manager, Employee")]
        [HttpPut("DutyDetail")]
        public async Task<IActionResult> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetailDto dto)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _dutyService.UpdateDutyDetailAsync(dto, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.DutyDetailResultDto>.ReturnResult("Update duty success", result, 200));
        }

        //// <summary>
        ///// Đánh dấu là hoàn tất chi tiết công việc, employee có thể đánh dấu công việc của mình miễn là trước hạn, manager có thể đánh dấu bất kể trước hay sau hạn
        //// </summary>
        //[Authorize(Roles = "Administrator, Manager, Employee")]
        //[HttpPut("MarkCompleted/{dutyDetailId}")]
        //public async Task<IActionResult> MarkDutyDetailAsCompletedAsync(Guid dutyDetailId)
        //{
        //    var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
        //        return Unauthorized("UserId invalid");

        //    var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        //    var result = await _dutyService.MarkDutyDetailAsCompletedAsync(dutyDetailId, currentUserId, currentUserRoles);

        //    return Ok(ApiResponse<string>.ReturnResult("Marked duty detail as completed", result, 200));
        //}

        // <summary>
        /// Xóa công việc, do admin/manager xử lý 
        // </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("dutyId")]
        public async Task<IActionResult> SoftDeleteDutyAsync([FromForm] Guid dutyId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = await _dutyService.SoftDeleteDutyAsync(dutyId, currentUserId, currentUserRoles);
            return Ok(ApiResponse<string>.ReturnResult("Delete duty success", result, 200));
        }
        // <summary>
        /// Xóa chi tiết công việc, do admin/manager xử lý 
        // </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpDelete("detailId")]
        public async Task<IActionResult> SoftDeleteDutyDetailAsync([FromForm] Guid detailId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = await _dutyService.SoftDeleteDutyDetailAsync(detailId, currentUserId, currentUserRoles);
            return Ok(ApiResponse<string>.ReturnResult("Delete duty success", result, 200));
        }
    }
}
