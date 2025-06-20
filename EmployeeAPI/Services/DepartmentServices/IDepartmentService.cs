using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;

namespace EmployeeAPI.Services.DepartmentServices
{
    public interface IDepartmentService
    {
        public Task<PagedResult<ResponseModel.DepartmentResultDto>> GetAllAsync(string? name, int? pageIndex, int? pageSize);
        public Task<ResponseModel.DepartmentResultDto> GetByIdAsync(Guid id);
        public Task<ResponseModel.DepartmentResultDto> AddAsync(string Name);
        public Task<ResponseModel.DepartmentResultDto> UpdateAsync(Guid id, string Name);
        public Task<string> SoftDeleteAsync(Guid id);
        public Task<PagedResult<UserFilterDto>> GetStaffByDepartmentAsync(Guid? departmentId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
        public Task<PagedResult<ResponseModel.PositionByDepartmentDto>> GetListPositionAsync(Guid? departmentId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
    }
}