using Microsoft.AspNetCore.Mvc;

using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Services.PositionServices;
using Microsoft.AspNetCore.Authorization;
using EmployeeAPI.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using static EmployeeAPI.Services.AuthServices.ResponseModel;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;
        private readonly ILogger<PositionController> _logger;

        public PositionController(IPositionService positionService, ILogger<PositionController> logger)
        {
            _positionService = positionService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách chức vụ, chưa authorize
        /// </summary>
        [HttpGet/*, Authorize*/]
        public async Task<IActionResult> GetAllPositions(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                var result = await _positionService.GetAllAsync(name, pageIndex, pageSize);

                return Ok(ApiResponse<PagedResult<ResponseModel.PositionDTO>>.ReturnResult("Get list position success", result, 200));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetAllPositions controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /*[HttpGet("id"), Authorize]
        public async Task<IActionResult> GetPositionById(Guid id)
        {
            var position = await _positionService.GetByIdAsync(id);
            if (position == null) return NotFound();
            return Ok(position);
        }*/

        /// <summary>
        /// Thêm chức vụ trong phỏng ban, chưa authorize
        /// </summary>
        [HttpPost/*, Authorize*/]
        public async Task<IActionResult> AddPosition([FromQuery] ResponseModel.CreatePosition dto)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(name)) return BadRequest("Position name cannot be empty");
                var result = await _positionService.AddAsync(dto);
                return Ok(ApiResponse<ResponseModel.PositionDTO>.ReturnResult("Create position success", result, 200));
            }
            catch(ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in AddPosition");
                return StatusCode(400, new { Message = "Add position failed", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in AddPosition");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in AddPosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// cập nhật chức vụ trong phòng ban, chưa authorize
        /// </summary>
        [HttpPut/*, Authorize*/]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromQuery] string newName)
        {
            try
            {
                if (id == Guid.Empty || string.IsNullOrWhiteSpace(newName)) 
                    return BadRequest(ApiResponse<string>.ReturnResult("Invalid input", null, 404));

                var result = await _positionService.UpdateAsync(id, newName);
                if (result == null)
                    return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the position id", null, 404));

                return Ok(ApiResponse<ResponseModel.UpdatePosition>.ReturnResult("Update position success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in AddPosition");
                return StatusCode(400, new { Message = "Position cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in UpdatePosition");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UpdatePosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }

        /// <summary>
        /// Xóa mềm chức vụ trong phòng ban, chưa authorize
        /// </summary>
        [HttpDelete/*, Authorize*/]
        public async Task<IActionResult> SoftDeletePosition([FromQuery] Guid id)
        {
            try
            {
                var result = await _positionService.SoftDeleteAsync(id);
                if (result == null) return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the position id", null, 404));
                return Ok(ApiResponse<string>.ReturnResult("Soft delete position success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentNullException in SoftDeletePosition");
                return StatusCode(400, new { Message = "Position cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeletePosition");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SoftDeletePosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Position id", StatusCode = 500 });
            }
        }

        /// <summary>
        /// lấy danh sách nhân viên theo chức vụ cảu 1 phòng ban, chưa authorize
        /// </summary>
        [HttpGet("Employee")/*, Authorize*/]
        public async Task<IActionResult> GetEmployeeByPosition(Guid DepartmentId, Guid PositionId, int? pageSize, int? pageIndex)
        {
            try
            {
                var pagedResult = await _positionService.GetStaffByPositionAsync(DepartmentId, PositionId, pageSize, pageIndex);
                if (pagedResult == null) return BadRequest(ApiResponse<string>.ReturnResult("Cannot find the Position name", null, 404));

                if (pagedResult.Items.Count() == 0)
                {
                    return BadRequest(ApiResponse<object>.ReturnResult("Cannot find the Position name", null, 400));
                }

                return Ok(ApiResponse<PagedResult<UserFilter>>.ReturnResult("Get list employee by position success", pagedResult, 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetEmployeeByPosition controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Position name", StatusCode = 500 });
            }
        }
    }
}
