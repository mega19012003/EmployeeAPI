using System.Runtime.InteropServices;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.AllowedIpServices;
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

        [HttpGet]
        public async Task<IActionResult> GetAllAllowedIP(string? ip, int? pageIndex, int? pageSize)
        {
            try
            {
                var result = await _allowedIPService.GetAllAllowedIPsAsync(ip, pageSize, pageIndex);
                return Ok(ApiResponse<PagedResult<AllowedIP>>.ReturnResult("Get all allowed IPs successfully", result, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving allowed IPs");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddAllowedIP([FromBody] AllowedIP model)
        {
            try
            {
                var result = await _allowedIPService.AddAllowedIPAsync(model.IPAddress);
                return Ok(ApiResponse<AllowedIP>.ReturnResult("Add allowed IP successfully", result, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding allowed IP");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }
    }
}
