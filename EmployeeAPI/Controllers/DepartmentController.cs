using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Services.DepartmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EmployeeAPI.Services.UserService;
using ResponseModel = EmployeeAPI.Services.DepartmentServices.ResponseModel;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;
using static EmployeeAPI.Services.UserService.ResponseModel;
using EmployeeAPI.Attributes;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerGroupOrder(3)]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentController> _logger;
        private readonly IUserService _userService;

        public DepartmentController(IDepartmentService departmentService, IUserService userService, ILogger<DepartmentController> logger)
        {
            _departmentService = departmentService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách phòng ban, manager/employee ko dc phép truy cập
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAll(string? name, int? pageIndex, int? pageSize)
        {
            var pagedResult = await _departmentService.GetAllAsync(name, pageIndex, pageSize);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<ResponseModel.DepartmentResultDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.DepartmentResultDto>>.ReturnResult("Get list department success", pagedResult, 200));
        }

        /// <summary>
        /// Lấy phòng ban theo Id
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet("{departmentId}")]
        public async Task<IActionResult> GetById(Guid departmentId)
        {
            var result = await _departmentService.GetByIdAsync(departmentId);
            if (result == null) return NotFound(ApiResponse<string>.ReturnResult("Department not found", null, 404));
            return Ok(ApiResponse<ResponseModel.DepartmentResultDto>.ReturnResult("Get department by Id success", result, 200));
        }

        /// <summary>
        /// Thêm phòng ban, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> AddDepartment([FromQuery] String Name)
        {
            var result = await _departmentService.AddAsync(Name);
            return Ok(ApiResponse<ResponseModel.DepartmentResultDto>.ReturnResult("Department added success", result, 200));
        }

        /// <summary>
        /// Cập nhật phòng ban,  do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public async Task<IActionResult> UpdateDepartment([FromQuery] Guid id, [FromQuery] string newName)
        {
            var result = await _departmentService.UpdateAsync(id, newName);
            return Ok(ApiResponse<ResponseModel.DepartmentResultDto>.ReturnResult("Updated Department Success", result, 200));
        }

        /// <summary>
        /// Xóa phòng ban, do admin xử lý
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{departmentId}")]
        public async Task<IActionResult> SoftDeleteDepartment(Guid departmentId)
        {
            var result = await _departmentService.SoftDeleteAsync(departmentId);
            //if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("", result, 400));
            return Ok(ApiResponse<string>.ReturnResult("Delete department success", result, 200));
        }

        /// <summary>
        /// lấy danh sách nhân viên theo phòng ban, manager sẽ lấy nhan viên theo phòng ban của mình
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeByDepartment(Guid departmentId, int? pageSize, int? pageIndex)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _departmentService.GetStaffByDepartmentAsync(departmentId, pageSize, pageIndex, currentUserId, currentUserRoles);

            if (!pagedResult.Items.Any())
                return Ok(ApiResponse<PagedResult<UserFilterDto>>.ReturnResult("No result", pagedResult, 200));

            return Ok(ApiResponse<PagedResult<ResponseModel.UserFilterDto>>.ReturnResult("Get list User by department success", pagedResult, 200));
        }

        /// <summary>
        /// lấy danh sách chức vụ có trong phòng ban, manager sẽ lấy chức vụ theo phòng ban của mình
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpGet("position")]
        public async Task<IActionResult> GetPositionsByDepartmentAsync(Guid DepartmentId, int? pageSize, int? pageIndex)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized("UserId invalid");

            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var pagedResult = await _departmentService.GetListPositionAsync(DepartmentId, pageSize, pageIndex, currentUserId, currentUserRoles);

            return Ok(ApiResponse<PagedResult<PositionByDepartmentDto >>.ReturnResult("Get list posistion by department success", pagedResult, 200));
        }
    }
}
