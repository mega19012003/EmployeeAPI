using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
using EmployeeAPI.Services.AllowedIpServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllowedIPController : ControllerBase
    {
        private readonly IAllowedIPService _allowedIPService;
        private readonly ILogger<AllowedIPController> _logger;
        public AllowedIPController(IAllowedIPService allowedIPService, ILogger<AllowedIPController> logger)
        {
            _allowedIPService = allowedIPService;
            _logger = logger;
        }
        /// <summary>
        ///  Lấy danh sách ip, system admin dc phép lấy toàn bộ cấu hình IP, admin/manager/employee chỉ dc phép lấy cấu hình theo công ty
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Search, Guid? companyId, int? pageIndex, int? pageSize)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pageResult = await _allowedIPService.GetAllAsync(Search, companyId, pageIndex, pageSize, currentUserId, currentUserRoles);
            return Ok(ApiResponse<PagedResult<ResponseModel.IPDto>>.ReturnResult("Get ip success", pageResult, 200));
        }

        /// <summary>
        ///  Lấy ip, system admin dc phép lấy toàn bộ cấu hình IP, admin/manager/employee chỉ dc phép lấy cấu hình theo công ty
        /// </summary>
        [Authorize]
        [HttpGet("{IPAddressId}")]
        public async Task<IActionResult> GetById(Guid IPAddressId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pageResult = await _allowedIPService.GetByIdAsync(IPAddressId, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.IPDto>.ReturnResult("Get ip success", pageResult, 200));
        }

        /// <summary>
        ///  Chỉ có admin dc phép dùng
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] string IPAddress)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _allowedIPService.AddAsync(IPAddress, currentUserId, currentUserRoles);
            return Ok(ApiResponse<ResponseModel.IPDto>.ReturnResult("Add new IP success", result, 200));
        }
        /// <summary>
        ///  Chỉ có admin dc phép dùng
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{ipId}")]
        public async Task<IActionResult> Delete(Guid ipId)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _allowedIPService.DeleteAsync(ipId, currentUserId, currentUserRoles);
            return Ok(ApiResponse<string>.ReturnResult("Delete IP success", result, 200));
        }
    }
}
