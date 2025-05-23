using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Services.AuthServices;
using EmployeeAPI.Base;
using Azure;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using static EmployeeAPI.Services.AuthServices.ResponseModel;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }
        //nhớ thêm authorize
        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] ResponseModel.RegisterDto dto)
        {
            try
            {
                /*if (ModelState.IsValid == false)
                    return BadRequest(ModelState);
                if (string.IsNullOrEmpty(dto.Username))
                    return BadRequest("username  not allow null");
                if (string.IsNullOrEmpty(dto.Password))
                    return BadRequest("password not allow null");
                if (string.IsNullOrEmpty(dto.Fullname))
                    return BadRequest("fullname not allow null");*/

                var result = await _authService.RegisterAsync(dto);
                if (result == null)
                    return StatusCode(401, new { Message = "Register user failed", Detail = "null", StatusCode = 401 }); 

                return Ok(ApiResponse<ResponseModel.UserDto>.ReturnResult("Register success", result, 200));
            }
            catch (ArgumentException argEx)
            { 
                return StatusCode(400, new {Message = "Register User Failed", Detail  = argEx.Message, StatusCode = 400});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] ResponseModel.LoginDto dto)
        {
            try
            {
                /*if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                    return BadRequest("Email and password cannot be null");*/
                var user = await _authService.LoginAsync(dto.Username, dto.Password);
                if (user == null)
                    return BadRequest(new ApiResponse<ResponseModel.LoginDto>
                    {
                        Message = "Invalid username or password",
                        Data = null,
                        StatusCode = 400,
                    });

                var jwtSection = _configuration.GetSection("Jwt");
                var issuers = jwtSection["Issuer"];
                var audiences = jwtSection["Audience"];
                var keys = jwtSection["Key"];
                var expires = int.Parse(jwtSection["Expire"]);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.GivenName, user.Fullname ?? ""),
                    new Claim("RoleName", user.Role.ToString() ?? ""),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
                };
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keys));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var jwtSecurityToken = new JwtSecurityToken(
                    issuer: issuers,
                    audience: audiences,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expires),
                    signingCredentials: signinCredentials
                );
                var jwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
  
                return Ok(new ApiResponse<string>
                {
                    Message = "Login success",
                    Data = jwt,
                    StatusCode = 200,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("id")/*, Authorize*/]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAsync([FromForm] ResponseModel.UpdateUser dto)
        {
            try
            {
                var result = await _authService.UpdateAsync(dto);

                return Ok(ApiResponse<ResponseModel.UserDto>.ReturnResult("Update staff success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in UpdateAsync");
                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in UpdateAsync");
                return StatusCode(400, ApiResponse<string>.ReturnResult("Database update error", dbEx.InnerException?.Message ?? dbEx.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UpdateAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message, StatusCode = 500 });
            }
        }
        [HttpDelete/*, Authorize*/]
        public async Task<IActionResult> SoftDeleteAsync([FromForm] Guid Id)
        {
            try
            {
                var result = await _authService.SoftDeleteAsync(Id);
                /*if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);*/
                return Ok(ApiResponse<string>.ReturnResult("Soft delete staff success", result, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in SoftDeleteAsync");
                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeleteAsync");
                return BadRequest(new { Message = "Database update failed", Detail = dbEx.InnerException?.Message ?? dbEx.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SoftDeleteAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Staff Id", StatusCode = 500 });
            }
        }

        [HttpGet("GetAll"), /*, Authorize*/]
        public async Task<IActionResult> GetAllUserAsync(string? Name, Guid? departmentId, int? pageSize, int? pageIndex)
        {
            try
            {
                var pagedResult = await _authService.GetAllAsync(Name, departmentId, pageSize, pageIndex);
                if (pagedResult == null) {
                    return BadRequest();
                }
                
                return Ok(ApiResponse<PagedResult<UserDto>>.ReturnResult("Get list user success", pagedResult, 200));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "ArgumentException in SoftDeleteAsync");
                return StatusCode(400, new { Message = "Staff cannot be found", Detail = argEx.Message, StatusCode = 400 });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException in SoftDeleteAsync");
                return BadRequest(new { Message = "Database update failed", Detail = dbEx.InnerException?.Message ?? dbEx.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SoftDeleteAsync controller method.");
                return StatusCode(500, new { Message = "Internal server error", Detail = "Cannot find Staff Id", StatusCode = 500 });
            }
        }

        [HttpGet("TestEncryptPassword")]
        public IActionResult Get([FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return BadRequest("Password is required");
            var jwtSection = _configuration.GetSection("Jwt");

            var key = jwtSection["Key"];
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hash = Convert.ToBase64String(hashBytes);

            return Ok(hash);
        }

        [HttpGet("current")/*, Authorize*/]
        public IActionResult GetCurrentUser()
        {
            var user = HttpContext.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Message = "Unauthorized",
                    Data = null,
                    StatusCode = 401,
                });
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var fullname = user.FindFirst(ClaimTypes.GivenName)?.Value;
            var phone = user.FindFirst(ClaimTypes.MobilePhone)?.Value;

            var roleName = user.FindFirst("RoleName")?.Value;


            //// Chuyển đổi kiểu dữ liệu nếu cần
            //DateOnly? dateOfBirth = null;
            //if (DateOnly.TryParse(dobString, out var dobParsed))
            //    dateOfBirth = dobParsed;

            //double.TryParse(salaryString, NumberStyles.Any, CultureInfo.InvariantCulture, out var basicSalary);

            return Ok(new ApiResponse<object>
            {
                Message = "Get login user success",
                Data = new
                {
                    UserId = userId,
                    Username = username,
                    Fullname = fullname,
                    PhoneNumber = phone,
                    //Address = address,
                    RoleName = roleName,
                    //DateOfBirth = dateOfBirth,
                    //DepartmentName = department,
                    //PositionName = position,
                    //BasicSalary = basicSalary,
                },
                StatusCode = 200,
            });
        }

    }
}
