using System;
using System.Security.Cryptography;
using System.Text;
using EmployeeAPI.Base;
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

        public async Task<ResponseModel.UserDto> RegisterAsync(ResponseModel.RegisterDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _repository.GetUserByName(dto.Username);
                if (result != null)
                    throw new ArgumentException("User already existed");

                var department = await _context.Departments
                    .Include(d => d.Positions)  
                    .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId);

                if (department == null)
                    throw new ArgumentException("Department not found");

                bool isPositionInDepartment = department.Positions.Any(p => p.Id == dto.PositionId);

                if (!isPositionInDepartment)
                    throw new ArgumentException("Position does not belong to the specified Department");

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var imagePath = await _fileService.SaveFileAsync(dto.ImageUrl, uploadsFolder);

                var entity = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = dto.Username,
                    Fullname = dto.Fullname,
                    Password = HashPassword(dto.Password),
                    Role = dto.Role,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,
                    PhoneNumber = dto.PhoneNumber,
                    BasicSalary = dto.BasicSalary,
                    DepartmentId = dto.DepartmentId,
                    PositionId = dto.PositionId,
                    ImageUrl = imagePath
                };

                _context.Users.Add(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.UserDto
                {
                    userId = entity.UserId,
                    Username = entity.Username,
                    Fullname = entity.Fullname,
                    Address = entity.Address,
                    DateOfBirth = entity.DateOfBirth,
                    RoleName = entity.Role.ToString(),
                    PhoneNumber = entity.PhoneNumber,
                    BasicSalary = entity.BasicSalary,
                    DepartmentName = entity.Department.Name,
                    PositionName = entity.Position.Name,
                    ImageUrl = entity.ImageUrl,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding new staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
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

        public async Task<ResponseModel.UserDto> UpdateAsync(ResponseModel.UpdateUser dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                var existingUser = await _repository.GetByIdAsync(dto.UserId);
                if (existingUser == null) throw new ArgumentException("Cannot find User id");

                var imagePaths = await _fileService.UpdateFileAsync(dto.ImageUrl, uploadsFolder, existingUser.ImageUrl);

                var department = await _context.Departments
                  .Include(d => d.Positions)
                  .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId);

                if (department == null)
                    throw new ArgumentException("Department not found");

                bool isPositionInDepartment = department.Positions.Any(p => p.Id == dto.PositionId);

                if (!isPositionInDepartment)
                    throw new ArgumentException("Position does not belong to the specified Department");

                existingUser.Fullname = dto.Fullname;
                existingUser.Address = dto.Address;
                existingUser.PhoneNumber = dto.PhoneNumber;
                existingUser.DateOfBirth = dto.DateOfBirth;
                existingUser.DepartmentId = dto.DepartmentId;
                existingUser.PositionId = dto.PositionId;
                existingUser.BasicSalary = dto.BasicSalary;
                existingUser.ImageUrl = imagePaths;
                existingUser.IsActive = dto.IsActive;

                await _repository.UpdateAsync(existingUser);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResponseModel.UserDto
                {
                    userId = existingUser.UserId,
                    Fullname = existingUser.Fullname,
                    RoleName = existingUser.Role.ToString(),
                    Address = existingUser.Address,
                    PhoneNumber = existingUser.PhoneNumber,
                    DateOfBirth = existingUser.DateOfBirth,
                    BasicSalary = existingUser.BasicSalary,
                    DepartmentName = existingUser.Department.Name,
                    PositionName = existingUser.Position.Name,
                    ImageUrl = existingUser.ImageUrl,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid Id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingUser = await _repository.GetByIdAsync(Id);
                if (existingUser == null)
                    throw new ArgumentException("Cannot find User id");

                //existingUser.IsDeleted = true;
                //existingUser.IsActive = false;

                await _repository.SoftDeleteAsync(existingUser);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return "Đã xóa user: " + existingUser.Fullname;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
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

        /// <summary>
        /// //////////////////////
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        public async Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _context.Users
                    .Include(c => c.Department)
                    .Include(c => c.Position)
                    //.Where(f => string.IsNullOrEmpty(SearchTerm) || f.Fullname.ToLower().Contains(SearchTerm.ToLower()))
                    .Where(p => !p.IsDeleted || p.IsActive);
                
                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.UserDto
                    {
                        userId = f.UserId,
                        Fullname = f.Fullname,
                        RoleName = f.Role.ToString(),
                        DateOfBirth = f.DateOfBirth,
                        Address = f.Address,
                        PhoneNumber = f.PhoneNumber,
                        DepartmentName = f.Department.Name,
                        PositionName = f.Position.Name,
                        BasicSalary = f.BasicSalary,
                        ImageUrl = f.ImageUrl,
                    })
                    .ToListAsync();

                return new PagedResult<ResponseModel.UserDto>
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all employee. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.UserDto> GetByIdAsync(Guid id)
        {
            var results = await _repository.GetByIdAsync(id);
            if (results == null)
                throw new ArgumentException("Cannot find User id");

            return new ResponseModel.UserDto
            {
                userId = results.UserId,
                Username = results.Username,
                Fullname = results.Fullname,
                RoleName = results.Role.ToString(),
                DateOfBirth = results.DateOfBirth,
                Address = results.Address,
                PhoneNumber = results.PhoneNumber,
                DepartmentName = results.Department.Name,
                PositionName = results.Position.Name,
                BasicSalary = results.BasicSalary,
                ImageUrl = results.ImageUrl,
            };
        }

        public async Task<ResponseModel.UserDto> GetLoginUserAsync(ResponseModel.UserDto dto)
        {
            var result = await _repository.GetLoginUserAsync(dto.Username);
            return new ResponseModel.UserDto
            {
                userId = result.UserId,
                Fullname = result.Fullname,
                Address = result.Address,
                DateOfBirth = result.DateOfBirth,
                PhoneNumber = result.PhoneNumber,
                RoleName = result.Role.ToString(),
                DepartmentName = result.Department.Name,
                PositionName = result.Position.Name,
                ImageUrl = result.ImageUrl,
                BasicSalary = result.BasicSalary,
            };
        }

    }
}
