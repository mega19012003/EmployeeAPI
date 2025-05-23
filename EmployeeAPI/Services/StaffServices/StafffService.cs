//using EmployeeAPI.Base;
//using EmployeeAPI.Models;
//using EmployeeAPI.Repositories.Departments;
//using EmployeeAPI.Repositories.Positions;
//using EmployeeAPI.Repositories.Staffs;
//using EmployeeAPI.Services.FileServices;
//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;

//namespace EmployeeAPI.Services.StaffServices
//{
//    public class StafffService : IStaffService
//    {
//        private readonly IStaffRepository _repository;
//        private readonly IPositionRepository _positionRepository;
//        private readonly IDepartmentRepository _departmentRepository;
//        private readonly IFileService _fileService;
//        private readonly AppDbContext _context;
//        private readonly ILogger<StafffService> _logger;
//        public StafffService(IStaffRepository repository, IFileService fileService, IPositionRepository positionRepository, IDepartmentRepository departmentRepository, AppDbContext context, ILogger<StafffService> logger)
//        {
//            _repository = repository;
//            _fileService = fileService;
//            _positionRepository = positionRepository;
//            _departmentRepository = departmentRepository;
//            _context = context;
//            _logger = logger;
//        }

//        public async Task<PagedResult<ResponseModel.StaffDto>> GetAllAsync(string? name, int? pageIndex, int? pageSize)
//        {
//            try
//            {
//                pageIndex ??= 1;
//                pageSize ??= 10;

//                var query = _context.Staffs
//                    .Include(c => c.Department)
//                    .Include(c => c.Position)
//                    .Where(f => string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower()))
//                    .Where(p => !p.IsDeleted);

//                var totalCount = await query.CountAsync();

//                var items = await query
//                    .Skip((pageIndex.Value - 1) * pageSize.Value)
//                    .Take(pageSize.Value)
//                    .Select(f => new ResponseModel.StaffDto
//                    {
//                        StaffId = f.Id,
//                        Name = f.Name,
//                        DateOfBirth = f.DateOfBirth,
//                        PhoneNumber = f.PhoneNumber,
//                        Address = f.Address,
//                        DepartmentId = f.DepartmentId,
//                        DepartmentName = f.Department.Name,
//                        PositionId = f.PositionId,
//                        PositionName = f.Position.Name,
//                        BasicSalary = f.BasicSalary,
//                        ImageUrl = f.ImageUrl,
//                        IsActive = f.IsActive
//                    })
//                    .ToListAsync();

//                return new PagedResult<ResponseModel.StaffDto>
//                {
//                    TotalCount = totalCount,
//                    PageIndex = pageIndex.Value,
//                    PageSize = pageSize.Value,
//                    Items = items
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while retrieving all staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
//                throw;
//            }
//        }

//        public async Task<ResponseModel.StaffDto> GetByIdAsync(Guid id)
//        {
//            var results = await _repository.GetByIdAsync(id);
//            if (results == null)
//                throw new ArgumentException("Cannot find staff id");

//            return new ResponseModel.StaffDto
//            {
//                StaffId = results.Id,
//                Name = results.Name,
//                DateOfBirth = results.DateOfBirth,
//                PhoneNumber = results.PhoneNumber,
//                Address = results.Address,
//                DepartmentId = results.DepartmentId,
//                PositionId = results.PositionId,
//                BasicSalary = results.BasicSalary,
//                ImageUrl = results.ImageUrl,
//            };
//        }

//        public async Task<ResponseModel.StaffDto> AddAsync(ResponseModel.CreateStaff dto)
//        {
//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
//                var imagePaths = await _fileService.SaveFilesAsync(dto.ImageUrl, uploadsFolder);

//                var staff = new Staff
//                {
//                    Id = Guid.NewGuid(),
//                    Name = dto.Name,
//                    DepartmentId = dto.DepartmentId,
//                    PositionId = dto.PositionId,
//                    DateOfBirth = dto.DateOfBirth,
//                    PhoneNumber = dto.PhoneNumber,
//                    Address = dto.Address,
//                    BasicSalary = dto.BasicSalary,
//                    ImageUrl = imagePaths,
//                    IsActive = true
//                };

//                var created = await _repository.AddAsync(staff);

//                await transaction.CommitAsync();


//                return new ResponseModel.StaffDto
//                {
//                    StaffId = created.Id,
//                    Name = created.Name,
//                    DateOfBirth = created.DateOfBirth,
//                    PhoneNumber = created.PhoneNumber,
//                    Address = created.Address,
//                    DepartmentId = created.DepartmentId,
//                    DepartmentName = (await _departmentRepository.GetByIdAsync(created.DepartmentId))?.Name,
//                    PositionId = created.PositionId,
//                    PositionName = (await _positionRepository.GetByIdAsync(created.PositionId))?.Name,
//                    BasicSalary = created.BasicSalary,
//                    ImageUrl = created.ImageUrl,
//                };
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                _logger.LogError(ex, "Error occurred while adding new staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
//                throw;
//            }
//        }

//        public async Task<ResponseModel.StaffDto> UpdateAsync(ResponseModel.UpdateStaff dto)
//        {
//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
//                var imagePaths = await _fileService.SaveFilesAsync(dto.ImageUrl, uploadsFolder);

//                var existingStaff = await _repository.GetByIdAsync(dto.Id);
//                if (existingStaff == null) throw new ArgumentException("Cannot find staff id");

//                existingStaff.Name = dto.Name;
//                existingStaff.DepartmentId = dto.DepartmentId;
//                existingStaff.PositionId = dto.PositionId;
//                existingStaff.BasicSalary = dto.BasicSalary;
//                existingStaff.ImageUrl = imagePaths;
//                existingStaff.IsActive = dto.IsActive;

//                await _repository.UpdateAsync(existingStaff);

//                await _context.SaveChangesAsync();

//                await transaction.CommitAsync();

//                return new ResponseModel.StaffDto
//                {
//                    Address = existingStaff.Address,
//                    BasicSalary = existingStaff.BasicSalary,
//                    DateOfBirth = existingStaff.DateOfBirth,
//                    DepartmentId = existingStaff.DepartmentId,
//                    DepartmentName = (await _departmentRepository.GetByIdAsync(existingStaff.DepartmentId))?.Name,
//                    ImageUrl = existingStaff.ImageUrl,
//                    Name = existingStaff.Name,
//                    PhoneNumber = existingStaff.PhoneNumber,
//                    PositionId = existingStaff.PositionId,
//                    PositionName = (await _positionRepository.GetByIdAsync(existingStaff.PositionId))?.Name,
//                    StaffId = existingStaff.Id,
//                    IsActive = existingStaff.IsActive
//                };
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                _logger.LogError(ex, "Error occurred while updating staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
//                throw;
//            }
//        }

//        public async Task<string> SoftDeleteAsync(Guid Id)
//        {
//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                var existingStaff = await _repository.GetByIdAsync(Id);
//                if (existingStaff == null)
//                    throw new ArgumentException("Cannot find staff id");

//                existingStaff.IsDeleted = true;
//                existingStaff.IsActive = false;

//                await _repository.SoftDeleteAsync(existingStaff);

//                await _context.SaveChangesAsync();

//                await transaction.CommitAsync();

//                return "Đã xóa user: " + existingStaff.Name;
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                _logger.LogError(ex, "Error occurred while deleting staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
//                throw;
//            }
//        }

//        public async Task<PagedResult<ResponseModel.StaffDto>> GetByNameAsync(string name, int? pageSize, int? pageIndex)
//        {
//            try
//            {
//                pageSize ??= 10;
//                pageIndex ??= 1;
//                var query = _context.Staffs
//                    .Include(c => c.Department)
//                    .Include(c => c.Position)
//                    .Where(f => string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower()))
//                    .Where(p => !p.IsDeleted);

//                var totalCount = await query.CountAsync();

//                var items = await query
//                    .Skip((pageIndex.Value - 1) * pageSize.Value)
//                    .Take(pageSize.Value)
//                    .Select(f => new ResponseModel.StaffDto
//                    {
//                        StaffId = f.Id,
//                        Name = f.Name,
//                        DateOfBirth = f.DateOfBirth,
//                        PhoneNumber = f.PhoneNumber,
//                        Address = f.Address,
//                        DepartmentId = f.DepartmentId,
//                        DepartmentName = f.Department.Name,
//                        PositionId = f.PositionId,
//                        PositionName = f.Position.Name,
//                        BasicSalary = f.BasicSalary,
//                        ImageUrl = f.ImageUrl
//                    })
//                    .ToListAsync();

//                return new PagedResult<ResponseModel.StaffDto>
//                {
//                    TotalCount = totalCount,
//                    PageIndex = pageIndex.Value,
//                    PageSize = pageSize.Value,
//                    Items = items
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while retrieving staff by name. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
//                throw;
//            }
//        }
//    }
//}
