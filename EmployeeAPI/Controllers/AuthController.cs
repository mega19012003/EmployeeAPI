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

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthRepository authRepository, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ResponseModel.RegisterDto dto)
        {
            try
            {
                if (ModelState.IsValid == false)
                    return BadRequest(ModelState);
                if (string.IsNullOrEmpty(dto.Username))
                    return BadRequest("username  not allow null");
                if (string.IsNullOrEmpty(dto.Password))
                    return BadRequest("password not allow null");
                if (string.IsNullOrEmpty(dto.Fullname))
                    return BadRequest("fullname not allow null");
                var user = new User
                {
                    Username = dto.Username,
                    Fullname = dto.Fullname
                };

                var result = await _authRepository.RegisterAsync(dto.Username, dto.Password, dto.Fullname);
                if (result == null)
                    return BadRequest(new ApiResponse<ResponseModel.RegisterDto>
                    {
                        Message = "User already exists",
                        Data = null,
                        StatusCode = 400,
                    });

                //return BadRequest("User already exists");

                /*return Ok(new { result.Username, result.Fullname });*/
                return Ok(new ApiResponse<ResponseModel.RegisterDto>
                {
                  Message = "Register success",
                  Data = result,
                  StatusCode = 200,
                });
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
                var user = await _authRepository.LoginAsync(dto.Username, dto.Password);
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
                    new Claim(ClaimTypes.Name, dto.Username),
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
                //return Ok(new { token = jwt });
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

        [HttpGet]
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

    }
}
