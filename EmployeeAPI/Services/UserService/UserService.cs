using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.AuthServices;
using EmployeeAPI.Services.FileServices;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeAPI.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        private readonly IFileService _fileService;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        public UserService(IUserRepository repository, IFileService fileService, AppDbContext context, ILogger<AuthService> logger)
        {
            _repository = repository;
            _fileService = fileService;
            _context = context;
            _logger = logger;
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

        /*private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }*/

        public async Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _repository.GetAll();

                if (!string.IsNullOrEmpty(SearchTerm))
                    query = query.Where(x => x.Fullname.ToLower().Contains(SearchTerm.ToLower()));

                if (departmentId.HasValue)
                    query = query.Where(x => x.DepartmentId == departmentId.Value);

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
    }
}
