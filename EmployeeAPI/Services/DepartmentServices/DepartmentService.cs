using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;


namespace EmployeeAPI.Services.DepartmentServices
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IDepartmentRepository repository, IUserRepository userRepository, AppDbContext context, ILogger<DepartmentService> logger)
        {
            _repository = repository;
            _userRepository = userRepository;
            _context = context;
            _logger = logger;
        }
        public async Task<PagedResult<ResponseModel.DepartmentResultDto>> GetAllAsync(string? name, Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var isAdmin = currentUserRoles.Contains("Administrator");

                if (isAdmin)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);

                    if (currentUser == null || currentUser.CompanyId == null)
                    {
                        throw new ArgumentException("Người dùng hiện tại chưa thuộc công ty nào. Vui lòng liên hệ system admin để cập nhật công ty.");
                    }

                    companyId = currentUser.CompanyId; 
                }

                var query = _context.Departments
                    .Where(f => string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower()))
                    .Where(p => !p.isDeleted);


                if (companyId.HasValue)
                {
                    query = query.Where(p => p.CompanyId == companyId.Value);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Include(p => p.Company)
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.DepartmentResultDto
                    {
                        DepartmentId = f.Id,
                        CompanyName = f.Company.Name,
                        Name = f.Name,
                    }).ToListAsync();
                return new PagedResult<ResponseModel.DepartmentResultDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.DepartmentResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Không thể tìm thấy phòng ban này");

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);

                if (currentUserRoles.Contains("Administrator"))
                {
                    if(currentUser.CompanyId == null)
                        throw new ArgumentException("Người dùng hiện tại chưa thuộc công ty nào. Vui lòng liên hệ system admin để cập nhật công ty.");
                    else if (currentUser.CompanyId != result.CompanyId)
                        throw new ArgumentException("Chỉ được phép lấy phòng ban có trong công ty");
                }

                return new ResponseModel.DepartmentResultDto
                {
                    DepartmentId = result.Id,
                    CompanyName = result.Company.Name,
                    Name = result.Name,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving department by ID. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.DepartmentResultDto> AddAsync(string name, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("Tên phòng ban không được phép rỗng");

                var isAdmin = currentUserRole.Contains("Administrator");

                Guid companyId = Guid.Empty;

                if (isAdmin)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ quản trị hệ thống để cập nhật công ty.");

                    companyId = currentUser.CompanyId.Value;
                }

                var model = new Department
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    Name = name,
                };

                await _repository.AddAsync(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DepartmentResultDto
                {
                    DepartmentId = model.Id,
                    Name = model.Name,
                    CompanyName = model.Company.Name
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.DepartmentResultDto> UpdateAsync(Guid id, string newName, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Không thể tìm thấy phòng ban này");

                var isAdmin = currentUserRole.Contains("Aministrator");

                if (isAdmin)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ quản trị hệ thống để cập nhật công ty.");
                    else if (currentUser.CompanyId != result.CompanyId)
                        throw new ArgumentException("Chỉ được phép cập nhật phòng ban có trong công ty");
                }

                result.Name = newName;

                await _repository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DepartmentResultDto
                {
                    DepartmentId = result.Id,
                    Name = result.Name,
                    CompanyName = result.Name
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Không thể tìm thấy phòng ban này");

                var isAdmin = currentUserRole.Contains("Aministrator");

                if (isAdmin)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ quản trị hệ thống để cập nhật công ty.");
                    else if (currentUser.CompanyId != result.CompanyId)
                        throw new ArgumentException("Chỉ được phép xóa phòng ban có trong công ty");
                }

                if (await _repository.HasUsersUsingDepartmentAsync(id))
                {
                    throw new InvalidOperationException("Không thể xóa phòng ban " + result.Name + " vì vẫn còn người dùng đang sử dụng.");
                }

                result.isDeleted = true;
                await _repository.SoftDeleteAsync(result.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa phòng ban " + result.Name;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
