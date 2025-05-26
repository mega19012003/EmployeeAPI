using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.AuthServices;
using EmployeeAPI.Services.FileServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;
using static EmployeeAPI.Services.UserService.ResponseModel;

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

        public async Task<ResponseModel.UserDto> AdminUpdateStaffAsync(ResponseModel.AdminUpdateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                var existingUser = await _repository.GetByIdAsync(dto.UserId);
                if (existingUser == null) throw new ArgumentException("User không tồn tại");

                if (dto.ImageUrl != null)
                {
                    existingUser.ImageUrl = await _fileService.UpdateFileAsync(dto.ImageUrl, uploadsFolder, existingUser.ImageUrl);
                }

                if (!string.IsNullOrWhiteSpace(dto.Fullname)) existingUser.Fullname = dto.Fullname;
                if (!string.IsNullOrWhiteSpace(dto.Address)) existingUser.Address = dto.Address;
                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) existingUser.PhoneNumber = dto.PhoneNumber;
                if (dto.BasicSalary != default) existingUser.BasicSalary = (double)dto.BasicSalary;
                existingUser.IsActive = dto.IsActive;

                if (dto.DepartmentId.HasValue)
                {
                    existingUser.DepartmentId = dto.DepartmentId;

                    if (dto.PositionId.HasValue)
                    {
                        var department = await _context.Departments
                            .Include(d => d.Positions)
                            .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId.Value);

                        if (department == null)
                            throw new ArgumentException("Department does not existed");

                        bool isValidPosition = department.Positions.Any(p => p.Id == dto.PositionId.Value);
                        if (!isValidPosition)
                            throw new ArgumentException("Position does not existed in department");
                    }
                }
                if (dto.PositionId.HasValue)
                {
                    existingUser.PositionId = dto.PositionId;
                }

                await _repository.UpdateAsync(existingUser);
                await _context.SaveChangesAsync();

                // Load lại navigation properties để lấy tên Department và Position
                await _context.Entry(existingUser).Reference(u => u.Department).LoadAsync();
                await _context.Entry(existingUser).Reference(u => u.Position).LoadAsync();

                await transaction.CommitAsync();

                return new ResponseModel.UserDto
                {
                    userId = existingUser.UserId,
                    Fullname = existingUser.Fullname,
                    RoleName = existingUser.Role.ToString(),
                    Address = existingUser.Address,
                    PhoneNumber = existingUser.PhoneNumber,
                    BasicSalary = existingUser.BasicSalary,
                    DepartmentName = existingUser.Department?.Name,
                    PositionName = existingUser.Position?.Name,
                    ImageUrl = existingUser.ImageUrl,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật nhân viên. Message: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<UserDto> ManagerUpdateStaffAsync(ResponseModel.ManagerUpdateDto dto, Guid managerId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                var existingUser = await _repository.GetByIdAsync(dto.UserId);
                if (existingUser == null) throw new ArgumentException("User không tồn tại");

                var manager = await _repository.GetByIdAsync(managerId);
                if (manager == null || manager.DepartmentId == null)
                    throw new ArgumentException("Manager không có phòng ban. Vui lòng liên hệ admin để thêm phòng ban.");

                if (!string.IsNullOrWhiteSpace(dto.Fullname)) existingUser.Fullname = dto.Fullname;
                if (!string.IsNullOrWhiteSpace(dto.Address)) existingUser.Address = dto.Address;
                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) existingUser.PhoneNumber = dto.PhoneNumber;
                if (dto.PositionId.HasValue) existingUser.PositionId = dto.PositionId.Value;
                //if (dto.BasicSalary.HasValue) existingUser.BasicSalary = dto.BasicSalary.Value;
                //if (dto.IsActive.HasValue) existingUser.IsActive = dto.IsActive.Value;

                if (dto.ImageUrl != null)
                {
                    existingUser.ImageUrl = await _fileService.UpdateFileAsync(dto.ImageUrl, uploadsFolder, existingUser.ImageUrl);
                }

                // Gán department từ manager
                existingUser.DepartmentId = manager.DepartmentId;

                // Lưu thay đổi
                await _repository.UpdateAsync(existingUser);
                await _context.SaveChangesAsync();

                // Load lại navigation properties để lấy tên phòng ban và chức vụ
                await _context.Entry(existingUser).Reference(u => u.Department).LoadAsync();
                await _context.Entry(existingUser).Reference(u => u.Position).LoadAsync();

                await transaction.CommitAsync();

                return new UserDto
                {
                    userId = existingUser.UserId,
                    Fullname = existingUser.Fullname,
                    RoleName = existingUser.Role.ToString(),
                    Address = existingUser.Address,
                    PhoneNumber = existingUser.PhoneNumber,
                    BasicSalary = existingUser.BasicSalary,
                    DepartmentName = existingUser.Department?.Name,
                    PositionName = existingUser.Position?.Name,
                    ImageUrl = existingUser.ImageUrl,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật nhân viên. Message: {Message}", ex.Message);
                throw;
            }
        }


        /*public async Task<UserDto> ManagerUpdateStaffAsync(ResponseModel.ManagerUpdateDto dto, Guid managerId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
               

                var existingUser = await _repository.GetByIdAsync(dto.UserId);
                if (existingUser == null) throw new ArgumentException("User không tồn tại");

                string imagePaths = existingUser.ImageUrl; // mặc định giữ ảnh cũ

                if (!string.IsNullOrWhiteSpace(dto.Fullname))
                    existingUser.Fullname = dto.Fullname;

                if (!string.IsNullOrWhiteSpace(dto.Address))
                    existingUser.Address = dto.Address;

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    existingUser.PhoneNumber = dto.PhoneNumber;

                if (dto.PositionId.HasValue)
                    existingUser.PositionId = dto.PositionId;

                //if (dto.BasicSalary.HasValue)
                //    existingUser.BasicSalary = dto.BasicSalary.Value;

                //if (dto.IsActive.HasValue)
                //    existingUser.IsActive = dto.IsActive.Value;


                if (dto.ImageUrl != null)
                {
                    imagePaths = await _fileService.UpdateFileAsync(dto.ImageUrl, uploadsFolder, existingUser.ImageUrl);
                }

                var manager = await _repository.GetByIdAsync(managerId);
                if (manager == null || manager.DepartmentId == null)
                    throw new ArgumentException("Manager does not have department, please contect admin to add department");


                existingUser.Fullname = dto.Fullname;
                existingUser.Address = dto.Address;
                existingUser.PhoneNumber = dto.PhoneNumber;
                //existingUser.DateOfBirth = dto.DateOfBirth;
                existingUser.PositionId = dto.PositionId;
                existingUser.BasicSalary = dto.BasicSalary;
                existingUser.ImageUrl = imagePaths;
                existingUser.IsActive = dto.IsActive;

                // Gán DepartmentId ngầm từ Manager
                existingUser.DepartmentId = manager.DepartmentId;

                //// Gán role ngầm là Staff (manager không được cấp role khác)
                //existingUser.Role = RoleType.Employee;
           
                await _repository.UpdateAsync(existingUser);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new UserDto
                {
                    userId = existingUser.UserId,
                    Fullname = existingUser.Fullname,
                    RoleName = existingUser.Role.ToString(),
                    Address = existingUser.Address,
                    PhoneNumber = existingUser.PhoneNumber,
                    //DateOfBirth = existingUser.DateOfBirth,
                    BasicSalary = existingUser.BasicSalary,
                    DepartmentName = existingUser.Department.Name,
                    PositionName = existingUser.Position.Name,
                    ImageUrl = existingUser.ImageUrl,
                };
            }
            catch (Exception ex)
            {
                if (transaction?.GetDbTransaction()?.Connection != null)
                {
                    await transaction.RollbackAsync();
                }
                _logger.LogError(ex, "Error occurred while updating User. Message: {Message}", ex.Message);
                throw;
            }
        }*/

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
 
                        Address = f.Address,
                        PhoneNumber = f.PhoneNumber,
                        /*DepartmentName = f.Department.Name,
                        PositionName = f.Position.Name,*/
                        DepartmentName = f.Department != null ? f.Department.Name : string.Empty,
                        PositionName = f.Position != null ? f.Position.Name : string.Empty,
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
         
                Address = results.Address,
                PhoneNumber = results.PhoneNumber,
                DepartmentId = results.DepartmentId,
                DepartmentName = results.Department?.Name ?? "No Department",
                PositionName = results.Position?.Name ?? "No Position",
                BasicSalary = results.BasicSalary,
                ImageUrl = results.ImageUrl,
            };
        }
    }
}
