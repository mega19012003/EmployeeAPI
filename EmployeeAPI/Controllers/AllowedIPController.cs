using System.Data.Common;
using System.Runtime.InteropServices;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
using EmployeeAPI.Services.AllowedIpServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        ///  Lấy danh sách ip, Chỉ có admin dc phép dùng
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? IpAdress, int? pageIndex, int? pageSize)
        {
            var pageResult = await _allowedIPService.GetAllAsync(IpAdress, pageIndex, pageSize);
            return Ok(ApiResponse<PagedResult<AllowedIP>>.ReturnResult("Get ip success", pageResult, 200));
        }

        /// <summary>
        ///  Chỉ có admin dc phép dùng
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string IPAddress)
        {
            var result = await _allowedIPService.AddAsync(IPAddress);
            return Ok(ApiResponse<AllowedIP>.ReturnResult("Add new IP success", result, 200));
        }
        /// <summary>
        ///  Chỉ có admin dc phép dùng
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("id")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _allowedIPService.DeleteAsync(id);
            return Ok(ApiResponse<string>.ReturnResult("Delete IP success", result, 200));
        }

    }
}
