using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Departments;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.AuthServices.ResponseModel;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;
using static EmployeeAPI.Services.PositionServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.DepartmentServices
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;
        private readonly IAuthRepository _authRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IDepartmentRepository repository, IAuthRepository authRepository, AppDbContext context, ILogger<DepartmentService> logger)
        {
            _repository = repository;
            _authRepository = authRepository;
            _context = context;
            _logger = logger;
        }
        public async Task<PagedResult<ResponseModel.DepartmentDto>> GetAllAsync(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;
                var query = _context.Departments
                    .Where(f => string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower()))
                    .Where(p => !p.isDeleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.DepartmentDto
                    {
                        DepartmentId = f.Id,
                        Name = f.Name,
                        IsDeleted = f.isDeleted
                    }).ToListAsync();
                return new PagedResult<ResponseModel.DepartmentDto>
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
        public async Task<ResponseModel.DepartmentDto> GetByIdAsync(Guid id)
        {
            try
            {
                var departmant = await _repository.GetByIdAsync(id);

                return new DepartmentDto
                {
                    DepartmentId = departmant.Id,
                    Name = departmant.Name,
                    IsDeleted = departmant.isDeleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving department by id. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.CreateDepartment> AddAsync(string name)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if(name ==null)
                {
                    throw new ArgumentNullException(nameof(name), "Department name cannot be null or empty");
                }

                var model = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                };

                /*var entity =*/
                await _repository.AddAsync(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CreateDepartment
                {
                    DepartmentId = model.Id,
                    Name = model.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.UpdateDepartment> UpdateAsync(Guid id, string newName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                /*if (id == Guid.Empty)
                    throw new ArgumentException("Department ID is invalid", nameof(id));

                if (string.IsNullOrWhiteSpace(newName))
                    throw new ArgumentException("Department name cannot be empty", nameof(newName));*/


                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find department id");

                result.Name = newName;

                await _repository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new UpdateDepartment
                {
                    DepartmentId = result.Id,
                    Name = result.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find department id");

                result.isDeleted = true;
                await _repository.SoftDeleteAsync(result.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa phòng ban: " + result.Name;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<PagedResult<UserFilter>> GetStaffByDepartmentAsync(Guid departmentId, int? pageSize, int? pageIndex)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _context.Departments
                    .Include(d => d.Users)
                    .Where(d => !d.isDeleted && d.Id == departmentId);

                var allStaffs = query
                    .SelectMany(d => d.Users
                        .Where(s => s.IsActive && !s.IsDeleted));

                var totalCount = await allStaffs.CountAsync();

                var items = await allStaffs
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(st => new UserFilter
                    {
                        UserId = st.UserId,
                        Name = st.Fullname,
                        BasicSalary = st.BasicSalary,
                        ImageUrl = st.ImageUrl,
                    })
                    .ToListAsync();

                return new PagedResult<UserFilter>
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving staff by department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<PagedResult<PositionByDepartment>> GetListPositionAsync(Guid departmentId, int? pageSize, int? pageIndex)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Departments
                .Include(d => d.Positions)
                .Where(d => !d.isDeleted);

            var listPosition = query
                .SelectMany(d => d.Positions
                .Where(s => s.DepartmentId == departmentId && !s.IsDeleted));

            var totalCount = await listPosition.CountAsync();

            var items = await listPosition
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(st => new PositionByDepartment
                {
                    Name = st.Name,
                    PositionId = st.Id,
                })
                .ToListAsync();

            return new PagedResult<PositionByDepartment>
            {
                TotalCount = totalCount,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                Items = items
            };

        }
    }
}
