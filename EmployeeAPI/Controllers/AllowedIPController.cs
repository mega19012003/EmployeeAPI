using System.Data.Common;
using System.Runtime.InteropServices;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;
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
        /// <summary>
        /// (đang fix)
        /// </summary>

        [HttpGet]
        public async Task<IActionResult> GetAllAllowedIP(string? ip, int? pageIndex, int? pageSize)
        {
            try
            {
                var result = await _allowedIPService.GetAllAllowedIPsAsync(ip, pageSize, pageIndex);
                return Ok(ApiResponse<PagedResult<ResponseModel.IPDto>>.ReturnResult("Get all allowed IPs successfully", result, 200));
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while retrieving allowed IPs");
                return StatusCode(500, new { Message = "Database error", Detail = dbEx.Message, StatusCode = 500 });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid argument provided while retrieving allowed IPs");
                return BadRequest(new { Message = "Invalid argument", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving allowed IPs");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// (đang fix)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddAllowedIP([FromForm] string IpAddress)
        {
            try
            {
                var result = await _allowedIPService.AddAllowedIPAsync(IpAddress);
                return Ok(ApiResponse< ResponseModel.IPDto>.ReturnResult("Add allowed IP successfully", result, 200));
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while adding allowed IPs");
                return StatusCode(500, new { Message = "Database error", Detail = dbEx.Message, StatusCode = 500 });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid argument provided while adding allowed IPs");
                return BadRequest(new { Message = "Invalid argument", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding allowed IPs");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// (đang fix)
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateAllowedIP([FromBody] ResponseModel.IPDto dto)
        {
            try
            {
                var result = await _allowedIPService.UpdateAllowedIPAsync(dto);
                return Ok(ApiResponse<ResponseModel.IPDto>.ReturnResult("Update allowed IP successfully", result, 200));
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while updating allowed IPs");
                return StatusCode(500, new { Message = "Database error", Detail = dbEx.Message, StatusCode = 500 });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid argument provided while updating allowed IPs");
                return BadRequest(new { Message = "Invalid argument", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating allowed IPs");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// (đang fix)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteAllowedIP([FromForm] Guid IPId)
        {
            try
            {
                var result = await _allowedIPService.DeleteAllowedIPAsync(IPId);
                return Ok(ApiResponse<string>.ReturnResult("Delete allowed IP successfully", result, 200));
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while deleting allowed IPs");
                return StatusCode(500, new { Message = "Database error", Detail = dbEx.Message, StatusCode = 500 });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid argument provided while deleting allowed IPs");
                return BadRequest(new { Message = "Invalid argument", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting allowed IPs");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// (đang fix)
        /// </summary>
    }
}
