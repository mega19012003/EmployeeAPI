using CloudinaryDotNet;
using EmployeeAPI.Base;
using EmployeeAPI.Services.CompanyServices;
using EmployeeAPI.Services.DepartmentServices;
using EmployeeAPI.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static EmployeeAPI.Services.CompanyServices.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        public readonly ICompanyService _companyService;
        private readonly ILogger<CompanyController> _logger;
        private readonly IUserService _userService;
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }


        /// <summary>
        /// Lấy thông tin công ty, SystemAdmin lấy dc toàn bộ công ty, Admin/manager/employee chỉ dc phép lấy công ty của mình
        /// </summary>
        [HttpGet("{companyId}")]
        [Authorize/*(Roles = "Administrator, Manager")*/]
        public async Task<IActionResult> GetCompanyById(Guid companyId)
        {

            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _companyService.GetCompanyByIdAsync(companyId ,currentUserId, currentUserRoles);

            return Ok(ApiResponse<CompanyResultDto>.ReturnResult("Get company success", result, 200));
        }

        /// <summary>
        /// Lấy danh sách công ty, chỉ admin được phép dùng 
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> GetAllCompanies(string? Name, int? pageIndex, int? pageSize)
        {
            //var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            //    return Unauthorized("User invalid");
            var pagedResult = await _companyService.GetAllCompaniesAsync(Name, pageIndex, pageSize);
            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<CompanyResultDto>>.ReturnResult("No result", pagedResult, 200));
            return Ok(ApiResponse<PagedResult<CompanyResultDto>>.ReturnResult("Get list company success", pagedResult, 200));
        }

        /// <summary>
        /// Thêm công ty mới, chỉ admin được phép dùng
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> CreateCompany([FromForm]CreateCompanyDto dto)
        {
            var result = await _companyService.CreateCompanyAsync(dto);
            return Ok(ApiResponse<CompanyResultDto>.ReturnResult("Create company success", result, 200));
        }

        /// <summary>
        /// Cập nhật công ty, chỉ admin được phép dùng
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> UpdateCompany([FromForm]UpdateCompanyDto dto)
        {
            var result = await _companyService.UpdateCompanyAsync(dto);
            return Ok(ApiResponse<CompanyResultDto>.ReturnResult("Update company success", result, 200));
        }

        /// <summary>
        /// Xóa công ty, chỉ admin được phép dùng
        /// </summary>
        [HttpDelete("{companyId}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var result = await _companyService.DeleteCompanyAsync(companyId);
            return Ok(ApiResponse<string>.ReturnResult("Delete company success", result, 200));
        }
    }
}
