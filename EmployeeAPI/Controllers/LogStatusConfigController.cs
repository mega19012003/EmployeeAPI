using CloudinaryDotNet;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.LogStatusConfigServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;
using static EmployeeAPI.Services.LogStatusConfigServices.ResponseModel;

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
        /// Lấy tất cả cấu hình trạng thái checkin, dùng để tính lương, systemAdmin lấy được mọi cấu hình
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Search, Guid? companyId, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var pagedResult = await _service.GetAllConfigsAsync(Search, companyId, pageIndex, pageSize, currentUserId, currentUserRoles);
            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.LogStatusDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.LogStatusDto>>.ReturnResult("Get all configs success", pagedResult, 200));
        }

        /// <summary>
        /// Lấy cấu hình trạng thái checkin theo id, systemAdmin lấy được mọi cấu hình, Admin lấy cấu hình của công ty
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, SystemAdmin")]
        [HttpGet("{StatusId}")]
        public async Task<IActionResult> GetById(Guid StatusId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var configs = await _service.GetConfigIdAsync(StatusId, currentUserId, currentUserRoles);

            return Ok(ApiResponse<ResponseModel.LogStatusDto>.ReturnResult("Get status configs success", configs, 200));
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
            return Ok(ApiResponse<LogStatusDto>.ReturnResult("Update config success", result, 200));
        }
    }
}
