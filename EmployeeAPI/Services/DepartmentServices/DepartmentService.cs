using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Staffs;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;
using static EmployeeAPI.Services.StaffServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.DepartmentServices
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;
        private readonly IStaffRepository _staffRepository;
        private readonly AppDbContext _context;
        public DepartmentService(IDepartmentRepository repository, IStaffRepository staffRepository, AppDbContext context)
        {
            _repository = repository;
            _staffRepository = staffRepository;
            _context = context;
        }
        public async Task<PagedResult<ResponseModel.DepartmentDto>> GetAllAsync(string? name, int? pageIndex, int? pageSize)
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

            /*var entity =*/ await _repository.AddAsync(model);
            return new ResponseModel.CreateDepartment
            {
                DepartmentId = model.Id,
                Name = model.Name,
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
            await _repository.UpdateAsync(result);
            return new UpdateDepartment
            {
                DepartmentId = result.Id,
                Name = result.Name,
            };
        }

        public async Task<string> SoftDeleteAsync(Guid id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null) return null;

            result.isDeleted = true;
            await _repository.SoftDeleteAsync(result.Id);
            //if (result == null) return null;

            return "Đã xóa phòng ban: " + result.Name;
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
