using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Services.FileServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;

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
                DepartmentName = p.Department?.Name ?? "Không xác định",
                PositionId = p.PositionId,
                PositionName = p.Position?.Name ?? "Không xác định",
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

        public async Task<ResponseModel.StaffDto> AddAsync(ResponseModel.CreateStaff dto)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var imagePaths = await _fileService.SaveFilesAsync(dto.ImageUrl, uploadsFolder);

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
                ImageUrl = imagePaths,
                IsActive = true
            };

            var created = await _repository.AddAsync(staff);

            // Map model sang DTO trả về
            return new ResponseModel.StaffDto
            {
                StaffId = created.Id,
                Name = created.Name,
                DateOfBirth = created.DateOfBirth,
                PhoneNumber = created.PhoneNumber,
                Address = created.Address,
                DepartmentId = created.DepartmentId,
                DepartmentName = (await _departmentRepository.GetByIdAsync(created.DepartmentId))?.Name,
                PositionId = created.PositionId,
                PositionName = (await _positionRepository.GetByIdAsync(created.PositionId))?.Name,
                BasicSalary = created.BasicSalary,
                ImageUrl = created.ImageUrl,
            };
        }


        public async Task<ResponseModel.StaffDto> UpdateAsync(ResponseModel.UpdateStaff dto)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var imagePaths = await _fileService.SaveFilesAsync(dto.ImageUrl, uploadsFolder);

            var existingStaff = await _repository.GetByIdAsync(dto.Id);

            existingStaff.Name = dto.Name;
            existingStaff.DepartmentId = dto.DepartmentId;
            existingStaff.PositionId = dto.PositionId;
            existingStaff.BasicSalary = dto.BasicSalary;

            existingStaff.ImageUrl = imagePaths;

            existingStaff.IsActive = dto.IsActive;

            await _repository.UpdateAsync(existingStaff);

            return new ResponseModel.StaffDto
            {
                Address = existingStaff.Address,
                BasicSalary = existingStaff.BasicSalary,
                DateOfBirth = existingStaff.DateOfBirth,
                DepartmentId = existingStaff.DepartmentId,
                ImageUrl = existingStaff.ImageUrl,
                Name = existingStaff.Name,
                PhoneNumber = existingStaff.PhoneNumber,
                PositionId = existingStaff.PositionId,
                StaffId = existingStaff.Id,
                IsActive = existingStaff.IsActive
            };
        }



        public async Task<string> SoftDeleteAsync(Guid Id)
        {
            var existingStaff = await _repository.SoftDeleteAsync(Id);
            if (existingStaff == null)
            {
                return null;
            }
            return "Đã xóa user: " + existingStaff.Name;
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
                DepartmentName = p.Department?.Name ?? "Không xác định",
                PositionId = p.PositionId,
                PositionName = p.Position?.Name ?? "Không xác định",
                BasicSalary = p.BasicSalary,
                ImageUrl = p.ImageUrl
            });
        }
    }
}
