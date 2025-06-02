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

using EmployeeAPI.Services.FileServices;
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
                var currentUsername = user.Identity.Name;
                var currentUser = await _repository.GetUserByName(currentUsername);

                if (currentUser == null)
                    throw new UnauthorizedAccessException("Current user not found");

                if (currentUser.Role != RoleType.Administrator && currentUser.Role != RoleType.Manager)
                    throw new UnauthorizedAccessException("Only Admin or Manager can register new users");

                if (currentUser.Role == RoleType.Manager && dto.Role != RoleType.Employee)
                    throw new UnauthorizedAccessException("Manager can only register Staff users");

                var existed = await _repository.GetUserByName(dto.Username);
                if (existed != null)
                    throw new ArgumentException("User already exists");

                var entity = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = dto.Username,
                    Password = HashPassword.ComputeHash(dto.Password),
                    Fullname = dto.Fullname,
                    Role = dto.Role,
                    PhoneNumber = "",
                    Address = "",
                    ImageUrl = "",
                    DepartmentId = null,
                    PositionId = null
                };

                _context.Users.Add(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.AuthDto
                {
                    userId = entity.UserId,
                    Username = entity.Username,
                    Fullname = entity.Fullname,
                    RoleName = entity.Role.ToString(),
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while registering user");
                throw;
            }
        }
        //private string HashPassword(string password)
        //{
        //    using (var sha256 = SHA256.Create())
        //    {
        //        var bytes = Encoding.UTF8.GetBytes(password);
        //        var hash = sha256.ComputeHash(bytes);
        //        return Convert.ToBase64String(hash);
        //    }
        //}
        public async Task<User> GetUserById(Guid userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");
            return user;
        }
        public async Task<User> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _repository.LoginAsync(username, password);
                if (user == null)
                    throw new Exception("Wrong Username or Password ");
                if(user.IsDeleted)
                    throw new Exception("UserAccount has been deleted");

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
                throw new ArgumentException("User not found");

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
                    throw new ArgumentException("User not found");

                if (user.Password != HashPassword.ComputeHash(oldPassword))
                    throw new ArgumentException("Password is incorrect");

                if (newPassword != confirmPassword)
                    throw new ArgumentException("New password and confirm password do not match");

                user.Password = HashPassword.ComputeHash(newPassword);
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
        public async Task<string> ResetPasswordAsync(Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _repository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("User not found");

                user.Password = HashPassword.ComputeHash("123456");

                await _repository.UpdateUserAsync(user);

                await transaction.CommitAsync();
                return "Reset password to 123456 success";
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
