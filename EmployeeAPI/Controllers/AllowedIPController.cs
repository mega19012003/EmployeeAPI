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
        /// Đang fix
        /// </summary>
  
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _allowedIPService.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Đang fix
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ip = await _allowedIPService.GetByIdAsync(id);
            if (ip == null) return NotFound();
            return Ok(ip);
        }
        /// <summary>
        /// Đang fix
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string IPAddress)
        {
            try
            {
                await _allowedIPService.AddAsync(IPAddress);
                return Ok(new { Message = "Thêm IP thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Đang fix
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _allowedIPService.DeleteAsync(id);
            return Ok(new { Message = "Xóa IP thành công" });
        }

    }
}
