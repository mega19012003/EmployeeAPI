//using Azure;
//using EmployeeAPI.Base;
//using EmployeeAPI.Repositories.Staffs;
//using EmployeeAPI.Services.StaffServices;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.EntityFrameworkCore;

//namespace EmployeeAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class StaffController : ControllerBase
//    {
//        private readonly IStaffService _staffService;
//        private readonly ILogger<StaffController> _logger;

//        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
//        {
//            _staffService = staffService;
//            _logger = logger;
//        }

//        [HttpGet, Authorize]
//        /*public async Task<IActionResult> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm)
//        {
//            var staff = await _staffService.GetAllAsync(pageSize, pageIndex, SearchTerm);
//            return Ok(staff);
//        }*/
//        public async Task<IActionResult> GetAllAsync(string? SearchTerm, int? pageSize, int? pageIndex)
//        {
//            try
//            {
//                var pagedResult = await _staffService.GetAllAsync(SearchTerm, pageSize, pageIndex);

//                /*if (pagedResult.Items.Count() == 0)
//                {
//                    return NotFound(new ApiResponse<object>
//                    {
//                        Message = "Cannot find staff",
//                        Data = null,
//                        StatusCode = 500
//                    });
//                }*/

//                return Ok(ApiResponse<PagedResult<ResponseModel.StaffDto>>.ReturnResult("Get list staff success", pagedResult, 200));
//            }
//            catch (Exception ex)
//            {
//                var response = new ApiResponse<string>
//                {
//                    Message = "An error occurred while retrieving staff",
//                    Data = ex.Message,
//                    StatusCode = 500
//                };
//                return StatusCode(500, response);
//            }
//        }

//        [HttpGet("Id"), Authorize]
//        public async Task<IActionResult> GetByIdAsync(Guid id)
//        {
//            try
//            {
//                var staff = await _staffService.GetByIdAsync(id);

//                return Ok(ApiResponse<ResponseModel.StaffDto>.ReturnResult("Get staff by id success", staff, 200));
//            }
//            catch (ArgumentException argEx)
//            {
//                _logger.LogError(argEx, "ArgumentException in GetByIdAsync");
//                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "DbUpdateException in GetByIdAsync");
//                return BadRequest(new { Message = "Database update failed", Detail = dbEx.InnerException?.Message ?? dbEx.Message, StatusCode = 400 });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Exception thrown in GetByIdAsync controller method.");
//                return NotFound(new { Message = "Internal server error", Detail = "Cannot find Staff Id", StatusCode = 500 });
//            }
//        }

//        [HttpPost]
//        [Consumes("multipart/form-data"), Authorize]
//        public async Task<IActionResult> AddAsync([FromForm] ResponseModel.CreateStaff dto)
//        {
//            if (dto == null)
//            {
//                return BadRequest("Invalid data.");
//            }
//            try
//            {
//                var result = await _staffService.AddAsync(dto);

//                //return Ok(result);
//                return Ok(ApiResponse<ResponseModel.StaffDto>.ReturnResult("Create staff success", result, 200));
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "DbUpdateException in AddAsync");

//                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

//                return BadRequest();/* new
//                {
//                    Message = "Database update failed",
//                    Detail = innerMessage,
//                    StatusCode = 400
//                });*/
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Exception thrown in AddAsync controller method.");
//                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
//            }
//        }

//        [HttpPut("id")]
//        [Consumes("multipart/form-data")]
//        public async Task<IActionResult> UpdateAsync([FromForm] ResponseModel.UpdateStaff dto)
//        {
//            try
//            {
//                var result = await _staffService.UpdateAsync(dto);

//                return Ok(ApiResponse<ResponseModel.StaffDto>.ReturnResult("Update staff success", result, 200));
//            }
//            catch (ArgumentException argEx)
//            {
//                _logger.LogError(argEx, "ArgumentException in UpdateAsync");
//                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "DbUpdateException in UpdateAsync");
//                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Exception thrown in UpdateAsync controller method.");
//                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
//            }
//        }

//        //[HttpPut("delete")]
//        [HttpDelete, Authorize]
//        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid Id)
//        {
//            try
//            {
//                var result = await _staffService.SoftDeleteAsync(Id);
//                /*if (result == null)
//                {
//                    return NotFound();
//                }
//                return Ok(result);*/
//                return Ok(ApiResponse<string>.ReturnResult("Soft delete staff success", result, 200));
//            }
//            catch (ArgumentException argEx)
//            {
//                _logger.LogError(argEx, "ArgumentException in SoftDeleteAsync");
//                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "DbUpdateException in SoftDeleteAsync");
//                return BadRequest(new { Message = "Database update failed", Detail = dbEx.InnerException?.Message ?? dbEx.Message, StatusCode = 400 });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Exception thrown in SoftDeleteAsync controller method.");
//                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Staff Id", StatusCode = 500 });
//            }
//        }

//        [HttpGet("name"), Authorize]
//        public async Task<IActionResult> GetByNameAsync(string name, int? pageSize, int? pageIndex)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(name))
//                {
//                    return BadRequest();
//                }
//                var result = await _staffService.GetByNameAsync(name, pageSize, pageIndex);
//                if (result == null)
//                {
//                    return BadRequest(/*new { Message = "Cannot find staff by name", Detail = "Staff not found", StatusCode = 400 }*/);
//                }

//                return Ok(ApiResponse<PagedResult<ResponseModel.StaffDto>>.ReturnResult("Get staff by name success", result, 200));

//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Exception thrown in GetByNameAsync controller method.");
//                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Staff by name", StatusCode = 500 });
//            }
//        }
//    }
//}
