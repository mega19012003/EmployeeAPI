using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeAPI.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IAuthRepository repository, IConfiguration configuration, AppDbContext context, ILogger<AuthService> logger)
        {
            _repository = repository;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task<ResponseModel.AuthDto> RegisterAsync(ResponseModel.RegisterDto dto, ClaimsPrincipal user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUserFullName = user.FindFirstValue("FullName");
                var currentUsername = user.Identity.Name;
                var currentUser = await _repository.GetUserByName(currentUsername);

                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy người dùng hiện tại");

                if (currentUser.Role == RoleType.Manager && dto.Role != RoleType.Employee)
                    //throw new UnauthorizedAccessException("Manager can only register employee user");
                    throw new ArgumentException("Manager chỉ có teh63 tạo user employee");

                var existed = await _repository.GetUserByName(dto.Username);
                if (existed != null)
                    throw new ArgumentException("Username đã tồn tại");

                if(!IsStrongPassword(dto.Password))
                    throw new ArgumentException("Password phải có ít nhất có 8 ký tự, gồm uppercase, lowercase, số và ký tự đặc biệt");

                var departmentId = Guid.Empty;
                if (currentUser.Role == RoleType.Manager && currentUser.DepartmentId != null)
                {
                    departmentId = currentUser.DepartmentId.Value;
                }

                //var generatedPassword = PasswordGenerator.Generate(8);

                var entity = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = dto.Username,
                    Password = HashPassword.Hash(dto.Password),
                    Fullname = dto.Fullname,
                    Role = dto.Role,
                    PhoneNumber = "",
                    Address = "",
                    ImageUrl = "",
                    PositionId = null,
                    SalaryPerHour = 0,
                };

                // Nếu currentUser là Manager → luôn gán DepartmentId
                if (currentUser.Role == RoleType.Manager && currentUser.DepartmentId.HasValue)
                {
                    entity.DepartmentId = currentUser.DepartmentId;
                }
                else
                {
                    // Admin: để null hoặc có thể gán dto.DepartmentId nếu bạn muốn
                    entity.DepartmentId = null;
                }

                _context.Users.Add(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.AuthDto
                {
                    userId = entity.UserId,
                    Username = entity.Username,
                    Fullname = entity.Fullname,
                    //Password = entity.Password, 
                    RoleName = entity.Role.ToString(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when registering user: {Message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                throw;
            }
        }
        public async Task<User> GetUserById(Guid userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Không tim thấy người dùng");
            return user;
        }
        public async Task<User> LoginAsync(string username, string password)
        {
            try 
            {
                var user = await _repository.LoginAsync(username, password);

                if (user.IsDeleted)
                    throw new ArgumentException("Người dùng này đã bị xóa");

                else if (user == null)
                    throw new ArgumentException("Không tìm thấy username");

                else if (HashPassword.Verify(user.Password, password) == false)
                //else if (user.Password != HashPassword.ComputeHash(password))
                    throw new ArgumentException("Sai password");

                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); 

                await _repository.UpdateUserAsync(user); 

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                throw;
            }
        }
        public async Task LogoutAsync(Guid userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Không tìm thấy người dùng");

            user.TokenVersion++;
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiryTime = DateTime.UtcNow;

            await _repository.UpdateUserAsync(user);
        }
        
        public async Task<string> ChangePasswordAsync(Guid userId, string oldPassword, string confirmPassword, string newPassword)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _repository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("Không tìm thấy người dùng");

                if (!HashPassword.Verify(user.Password, oldPassword))
                //if (user.Password != HashPassword.ComputeHash(oldPassword))
                    throw new ArgumentException("Password cũ không chính xác");

                if (newPassword != confirmPassword)
                    throw new ArgumentException("Password mới và xác nhận password mới nhập ko chính xác");

                if(!IsStrongPassword(newPassword))
                    throw new ArgumentException("Password mới phải có ít nhất có 8 ký tự, gồm uppercase, lowercase, số và ký tự đặc biệt");

                user.Password = HashPassword.Hash(newPassword);
                //user.Password = HashPassword.ComputeHash(newPassword);
                await _repository.UpdateUserAsync(user);
                await transaction.CommitAsync();
                return "Change password success";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while changing password");
                throw;
            }
        }

        public async Task<string> ResetPasswordAsync(Guid userId, ClaimsPrincipal claim)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUsername = claim.Identity?.Name;
                var currentUser = await _repository.GetUserByName(currentUsername);
                if (currentUser == null)
                    throw new ArgumentException("Không tìm thấy người dùng hiện tại");

                var user = await _repository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("Không tìm thấy người dùng");

                if (currentUser.Role == RoleType.Manager &&
                    user.DepartmentId != currentUser.DepartmentId)
                {
                    throw new ArgumentException("Manager chỉ có thể reset password cho user cùng phòng ban");
                }

               // var generatedPassword = PasswordGenerator.Generate(8);
                user.Password = HashPassword.Hash(user.Username);
                //user.Password = HashPassword.ComputeHash(generatedPassword);
                await _repository.UpdateUserAsync(user);
                await transaction.CommitAsync();

                return $"Reset password thành công. Password mới là username";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while resetting password");
                throw;
            }
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
        public async Task<string> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                throw new SecurityTokenException("Invalid access token");

            var username = principal.Identity.Name;
            var user = await _repository.GetUserByName(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new SecurityTokenException("Invalid refresh token");

            // Tạo token mới
            var jwt = GenerateAccessToken(user);

            // tạo refresh token mới
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _repository.UpdateUserAsync(user);

            return jwt;
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                return principal;

            return null;
        }
        public string GenerateAccessToken(User user)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));
            var signinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.Fullname ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString() ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["Expire"])),
                signingCredentials: signinCredentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            return password.Length >= 8
                && password.Any(char.IsUpper) 
                && password.Any(char.IsLower) 
                && password.Any(char.IsDigit) 
                && password.Any(ch => "!@#$%^&*()_-+=<>?".Contains(ch)); 
        }

        public static class PasswordGenerator
        {
            private static readonly Random _random = new Random();
            private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            private const string Lower = "abcdefghijklmnopqrstuvwxyz";
            private const string Digits = "0123456789";
            private const string Symbols = "!@#$%^&*()_-+=<>?";

            public static string Generate(int length = 8)
            {
                if (length < 8) throw new ArgumentException("Password too short");

                var chars = new List<char>
                {
                    Upper[_random.Next(Upper.Length)],
                    Lower[_random.Next(Lower.Length)],
                    Digits[_random.Next(Digits.Length)],
                    Symbols[_random.Next(Symbols.Length)]
                };

                string all = Upper + Lower + Digits + Symbols;
                while (chars.Count < length)
                    chars.Add(all[_random.Next(all.Length)]);

                return new string(chars.OrderBy(x => _random.Next()).ToArray());
            }
        }

        public async Task<ResponseModel.AuthDto> GetLoginUserAsync(ResponseModel.GetUserLogin dto)
        {
            var result = await _repository.GetLoginUserAsync(dto.UserName);
            return new ResponseModel.AuthDto
            {
                userId = result.UserId,
                Username = result.Username,
                Fullname = result.Fullname,
                RoleName = result.Role.ToString(),
            };
        }
    }
}
