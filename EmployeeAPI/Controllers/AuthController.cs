using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using EmployeeAPI.Services.AuthServices;
using EmployeeAPI.Base;
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

        /// <summary>
        /// Đăng ký người dùng, sẽ do admin/manager tạo, Manger chỉ dc phep tạo user với role là employee
        /// </summary>
        /// <remarks>         
        /// RoleType enum values:
        /// - 1 = Administrator
        /// - 2 = Manager
        /// - 3 = Employee
        /// </remarks> 

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ResponseModel.RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto, User);
                return Ok(ApiResponse<ResponseModel.AuthDto>.ReturnResult("Register success", result, 200));
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return BadRequest(new { message = "Register User Failed", detail = innerMessage, statusCode = 400 });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = "Access denied", detail = ex.Message, statusCode = 403 });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Register User Failed", detail = ex.Message, statusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user");
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message, statusCode = 500 });
            }
        }

        /// <summary>
        /// Đăng nhập người dùng
        /// </summary>
        /// <remarks>
        /// - TK admin
        ///{
        ///  "username": "Admin123",
        ///  "password": "anno123"
        ///}
        /// - Tk manager Thu ngân    { "username": "Manager123", "password": "anno123" }
        /// - Tk manager It          { "username": "Manager456", "password": "anno123" }
        /// - Tk manager sửa chữa    { "username": "Manager789", "password": "anno123" }
        /// - Tk employee Thu ngân 1 { "username": "user101", "password": "123456" }
        /// - Tk employee Thu ngân 2 { "username": "user01", "password": "123456" }
        /// - Tk employee IT         { "username": "user02", "password": "654321" }
        /// - Tk employee Sửa chữa   { "username": "user100", "password": "123456" }
        /// </remarks>
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
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.Fullname ?? ""),
                    new Claim(ClaimTypes.Role, user.Role.ToString() ?? ""),
                    new Claim("TokenVersion", user.TokenVersion.ToString() ?? ""),
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
  
                return Ok(new ApiResponse<object>
                {
                    Message = "Login success",
                    Data = new
                    {
                        AccessToken = jwt,
                        RefreshToken = user.RefreshToken
                    },
                    StatusCode = 200,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in user");
                return StatusCode(500, new ApiResponse<object>
                {
                    Message = "Internal server error",
                    Data = ex.Message,
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        [Authorize]
        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(new ApiResponse<string>
                    {
                        Message = "User not found in token",
                        StatusCode = 401
                    });

                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                    return BadRequest(new ApiResponse<string>
                    {
                        Message = "Invalid user ID",
                        StatusCode = 400
                    });

                await _authService.LogoutAsync(userId);

                return Ok(new ApiResponse<string>
                {
                    Message = "Logout successful",
                    StatusCode = 200
                });
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return BadRequest(new { message = "Logout User Failed", detail = innerMessage, statusCode = 400 });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<string>
                {
                    Message = ex.Message,
                    StatusCode = 404
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Làm mới token
        /// </summary>
        [Authorize]
        [HttpPost, Route("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
        {
            try
            {
                var newAccessToken = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

                return Ok(new ApiResponse<object>
                {
                    Message = "Token refreshed successfully",
                    Data = new { AccessToken = newAccessToken },
                    StatusCode = 200
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Message = ex.Message,
                    Data = null,
                    StatusCode = 401
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing token");
                return StatusCode(500, "Internal server error");
            }
        }


        /// <summary>
        /// Lấy thông tin user đang đăng nhập
        /// </summary>
        [Authorize]
        [HttpGet("current")/*, Authorize*/]
        public async Task<IActionResult> GetCurrentUser()
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
            var tokeVersion = user.FindFirst("TokenVersion")?.Value;

            var userEntity = await _authService.GetUserById(Guid.Parse(userId));
            if (userEntity == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Message = "User not found",
                    Data = null,
                    StatusCode = 401,
                });
            }

            //// So sánh TokenVersion trong token với tokenVersion trong DB
            //if (tokeVersion != userEntity.TokenVersion.ToString())
            //{
            //    return Unauthorized(new ApiResponse<string>
            //    {
            //        Message = "Token is no longer valid",
            //        Data = null,
            //        StatusCode = 401,
            //    });
            //}

            var userData = new
            {
                userEntity.UserId,
                userEntity.Username,
                userEntity.Fullname,
                Role = userEntity.Role.ToString(),
                userEntity.PhoneNumber,
                userEntity.Address,
                userEntity.PositionId,
                PositionName = userEntity.Position?.Name,
                userEntity.DepartmentId,
                DepartmentName = userEntity.Department?.Name,
                userEntity.BasicSalary,
                userEntity.IsActive,
                userEntity.IsDeleted,
                userEntity.ImageUrl
            };
            return Ok(new ApiResponse<object>
            {
                Message = "Get login user success",
                Data = userData,
                StatusCode = 200,
            });
        }

        /// <summary>
        /// Thay đổi password
        /// </summary>
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ResponseModel.ChangePasswordDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                    return BadRequest(new ApiResponse<string>
                    {
                        Message = "Invalid user ID",
                        StatusCode = 400
                    });
                var result = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.ConfirmPassword, dto.NewPassword);
                return Ok(ApiResponse<string>.ReturnResult("Change password success", result, 200));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Message = ex.Message,
                    StatusCode = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password");
                return StatusCode(500, new ApiResponse<string>
                {
                    Message = "Internal server error",
                    Data = ex.Message,
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Reset password, chỉ có admin/manager dc phép dùng
        /// </summary>
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] Guid id)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(id);
                return Ok(ApiResponse<string>.ReturnResult("Reset password success", result, 200));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Message = ex.Message,
                    StatusCode = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password");
                return StatusCode(500, new ApiResponse<string>
                {
                    Message = "Internal server error",
                    Data = ex.Message,
                    StatusCode = 500
                });
            }
        }
    }
}
