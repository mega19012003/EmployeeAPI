using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Services.FileServices;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EmployeeAPI.Services.StaffServices
{
    public class StafffService : IStaffService
    {
        private readonly IStaffRepository _repository;
        private readonly IPositionRepository _positionRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IFileService _fileService;
        public StafffService(IStaffRepository repository, IFileService fileService, IPositionRepository positionRepository, IDepartmentRepository departmentRepository)
        {
            _repository = repository;
            _fileService = fileService;
            _positionRepository = positionRepository;
            _departmentRepository = departmentRepository;
        }

        public async Task<IEnumerable<ResponseModel.StaffDto>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm)
        {
            var results = await _repository.GetAllAsync(pageSize, pageIndex, SearchTerm);
            return results.Select(p => new ResponseModel.StaffDto
            {
                StaffId = p.Id,
                Name = p.Name,
                DateOfBirth = p.DateOfBirth,
                PhoneNumber = p.PhoneNumber,
                Address = p.Address,
                DepartmentId = p.DepartmentId,
                PositionId = p.PositionId,
                BasicSalary = p.BasicSalary,
                ImageUrl = p.ImageUrl
            });
        }


        public async Task<ResponseModel.StaffDto> GetByIdAsync(Guid id)
        {
            var results = await _repository.GetByIdAsync(id);
            return new ResponseModel.StaffDto
            {
                StaffId = results.Id,
                Name = results.Name,
                DateOfBirth = results.DateOfBirth,
                PhoneNumber = results.PhoneNumber,
                Address = results.Address,
                DepartmentId = results.DepartmentId,
                PositionId = results.PositionId,
                BasicSalary = results.BasicSalary,
                ImageUrl = results.ImageUrl,
            };
        }

        public async Task<ResponseModel.CreateStaff> AddAsync(ResponseModel.CreateStaff dto)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var imagePaths = await _fileService.SaveFileAsync(dto.ImageUrl, uploadsFolder);

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                BasicSalary = dto.BasicSalary,
                ImageUrl = imagePaths 
            };

            var created = await _repository.AddAsync(staff);

            return new ResponseModel.CreateStaff
            {
                Name = created.Name,
                PhoneNumber = created.PhoneNumber,
                Address = created.Address,
                DepartmentId = created.DepartmentId,
                PositionId = created.PositionId,
                DateOfBirth = created.DateOfBirth,
                BasicSalary = created.BasicSalary,
            };
        }


        public async Task<ResponseModel.UpdateStaff> UpdateAsync(ResponseModel.UpdateStaff dto)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var imagePaths = await _fileService.SaveFileAsync(dto.ImageUrl, uploadsFolder);

            var existingStaff = await _repository.GetByIdAsync(dto.Id);
            existingStaff.Name = dto.Name;
            existingStaff.DepartmentId = dto.DepartmentId;
            existingStaff.PositionId = dto.PositionId;
            existingStaff.BasicSalary = dto.BasicSalary;
            existingStaff.ImageUrl = imagePaths;
            existingStaff.IsActive = dto.IsActive;

            await _repository.UpdateAsync(existingStaff);
            return new ResponseModel.UpdateStaff
            {
                Name = existingStaff.Name,
                DepartmentId = existingStaff.DepartmentId,
                PositionId = existingStaff.PositionId,
                BasicSalary = existingStaff.BasicSalary,
                IsActive = existingStaff.IsActive,
                ImageUrl = existingStaff.ImageUrl.Select(path => new FormFile(
                    new MemoryStream(File.ReadAllBytes(path)), 0, new FileInfo(path).Length, null, Path.GetFileName(path))
                ).ToList<IFormFile>()
            };
        }

        public async Task<ResponseModel.DeleteStaff> SoftDeleteAsync(Guid Id)
        {
            var existingStaff = await _repository.SoftDeleteAsync(Id);
            if (existingStaff == null)
            {
                return null;
            }
            return new ResponseModel.DeleteStaff
            {
                Id = existingStaff.Id,
                Name = existingStaff.Name,
                IsDeleted = existingStaff.IsDeleted,
            };
        }


        public async Task<IEnumerable<ResponseModel.StaffDto>> GetByNameAsync(string name, int? pageSize, int? pageIndex)
        {
            if (pageSize == null || pageSize <= 0)
            {
                pageSize = 10;
            }
            if (pageIndex == null || pageIndex <= 0)
            {
                pageIndex = 1;
            }
            var result = await _repository.GetByNameAsync(name, pageSize, pageIndex);
            return result.Select(p => new ResponseModel.StaffDto
            {
                StaffId = p.Id,
                Name = p.Name,
                DateOfBirth = p.DateOfBirth,
                PhoneNumber = p.PhoneNumber,
                Address = p.Address,
                DepartmentId = p.DepartmentId,
                PositionId = p.PositionId,
                BasicSalary = p.BasicSalary,
                ImageUrl = p.ImageUrl
            });
        }

        public async Task<IEnumerable<ResponseModel.StaffDto>> GetEmployeeByPosition(string SearchTerm, int? pageSize, int? pageIndex)
        {
            if (pageSize == null || pageSize <= 0)
            {
                pageSize = 10;
            }

            if (pageIndex == null || pageIndex <= 0)
            {
                pageIndex = 1;
            }

            var result = await _repository.GetEmployeeByPosition(SearchTerm, pageSize, pageIndex);
            return result.Select(p => new ResponseModel.StaffDto
            {
                StaffId = p.Id,
                Name = p.Name,
                DateOfBirth = p.DateOfBirth,
                PhoneNumber = p.PhoneNumber,
                Address = p.Address,
                DepartmentId = p.DepartmentId,
                PositionId = p.PositionId,
                BasicSalary = p.BasicSalary,
                ImageUrl = p.ImageUrl
            });
        }
    }
}
