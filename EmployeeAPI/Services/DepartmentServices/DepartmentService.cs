using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Staffs;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;
using static EmployeeAPI.Services.StaffServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.DepartmentServices
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;
        private readonly IStaffRepository _staffRepository;
        public DepartmentService(IDepartmentRepository repository, IStaffRepository staffRepository)
        {
            _repository = repository;
            _staffRepository = staffRepository;
        }
        public async Task<IEnumerable<ResponseModel.DepartmentDto>> GetAllAsync()
        {
            var departmant = await _repository.GetAllAsync();
            return departmant.Select(p => new DepartmentDto
            {
                DepartmentId = p.Id,
                Name = p.Name,
                IsDeleted = p.isDeleted
            });
        }
        public async Task<ResponseModel.DepartmentDto> GetByIdAsync(Guid id)
        {
            var departmant = await _repository.GetByIdAsync(id);
            return new DepartmentDto
            {
                DepartmentId = departmant.Id,
                Name = departmant.Name,
                IsDeleted = departmant.isDeleted
            };
        }
        public async Task<ResponseModel.CreateDepartment> AddAsync(string name)
        {

            var model = new Department
            {
                Id = Guid.NewGuid(),
                Name = name,
            };

            var entity = await _repository.AddAsync(model);
            return new ResponseModel.CreateDepartment
            {
                DepartmentId = entity.Id,
                Name = entity.Name,
            };
        }
        

        public async Task<ResponseModel.UpdateDepartment> UpdateAsync(Guid id, string newName)
        {
                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                {
                    return null;
                }
                result.Name = newName;
                var update = await _repository.UpdateAsync(result);
                if (update == null)
                {
                    return null;
                }
                return new UpdateDepartment
                {
                    DepartmentId = update.Id,
                    Name = update.Name,
                    IsDeleted = update.isDeleted,
                };
        }

        public async Task<string> SoftDeleteAsync(Guid id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
            {
                return null;
            }
            result.isDeleted = true;
            var update = await _repository.SoftDeleteAsync(result.Id);
            if (update == null)
            {
                return null;
            }
            return "Đã xóa phòng ban: " + id;
         }

        public async Task<IEnumerable<ResponseModel.DepartmentDto>> GetDepartmentByName(string name)
        {
            var result = await _repository.GetDepartmentByName(name);
            if (result == null)
            {
                return null;
            }
            return result.Select(d => new DepartmentDto
            {
                DepartmentId = d.Id,
                Name = d.Name,
                IsDeleted = d.isDeleted
            });
        }

        public async Task<IEnumerable<StaffFilter>> GetStaffByDepartmentAsync(string positionName, int? pageSize, int? pageIndex)
        {
            var staffs = await _repository.GetStaffByDepartmentAsync(positionName, pageSize, pageIndex);

            return staffs.SelectMany(pos => pos.Staffs
            .Where(st => st.IsActive && !st.IsDeleted))
            .Select(st => new StaffFilter
            {
                StaffId = st.Id,
                Name = st.Name,
                BasicSalary = st.BasicSalary,
                ImageUrl = st.ImageUrl,
            });
        }
    }
}
