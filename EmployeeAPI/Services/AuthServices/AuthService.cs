using System;
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EmployeeAPI.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;

        private readonly IFileService _fileService;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IAuthRepository repository, IFileService fileService, AppDbContext context, ILogger<AuthService> logger)
        {
            _repository = repository;
            _fileService = fileService;
            _context = context;
            _logger = logger;
        }

        //public async Task<ResponseModel.AuthDto> RegisterAsync(ResponseModel.RegisterDto dto)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        _logger.LogInformation("Checking if user exists: {Username}", dto.Username);
        //        var result = await _repository.GetUserByName(dto.Username);

        //        if (result != null)
        //        {
        //            _logger.LogWarning("User already existed: {Username}", dto.Username);
        //            throw new ArgumentException("User already existed");
        //        }

        //        //var department = await _context.Departments
        //        //    .Include(d => d.Positions)  
        //        //    .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId);

        //        //if (department == null)
        //        //    throw new ArgumentException("Department not found");

        //        //bool isPositionInDepartment = department.Positions.Any(p => p.Id == dto.PositionId);

        //        //if (!isPositionInDepartment)
        //        //    throw new ArgumentException("Position does not belong to the specified Department");

        //        //string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        //        //var imagePath = await _fileService.SaveFileAsync(dto.ImageUrl, uploadsFolder);

        //        var entity = new User
        //        {
        //            UserId = Guid.NewGuid(),
        //            Username = dto.Username,
        //            Fullname = dto.Fullname,
        //            Password = dto.Password,
        //            Role = dto.Role,
        //            PhoneNumber = "", 
        //            Address = "",
        //            DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
        //            ImageUrl = "",
        //            DepartmentId = null,
        //            PositionId = null
        //        };

        //        _context.Users.Add(entity);
        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return new ResponseModel.AuthDto
        //        {
        //            userId = entity.UserId,
        //            Username = entity.Username,
        //            Fullname = entity.Fullname,
        //            RoleName = entity.Role.ToString(),
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Error occurred while adding new staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
        //        throw;
        //    }
        //}
        public async Task<ResponseModel.AuthDto> RegisterAsync(ResponseModel.RegisterDto dto, ClaimsPrincipal user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUsername = user.Identity.Name;
                var currentUser = await _repository.GetUserByName(currentUsername);

                if (currentUser == null)
                    throw new UnauthorizedAccessException("Current user not found");

                // Chỉ cho Admin hoặc Manager được đăng ký user mới
                if (currentUser.Role != RoleType.Administrator && currentUser.Role != RoleType.Manager)
                    throw new UnauthorizedAccessException("Only Admin or Manager can register new users");

                var existed = await _repository.GetUserByName(dto.Username);
                if (existed != null)
                    throw new ArgumentException("User already exists");

                var entity = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = dto.Username,
                    Password = HashPassword(dto.Password), 
                    Fullname = dto.Fullname,
                    Role = dto.Role,
                    PhoneNumber = "",
                    Address = "",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _repository.LoginAsync(username, password);
                if (user == null)
                    throw new ArgumentException("Invalid input");

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding new staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
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
