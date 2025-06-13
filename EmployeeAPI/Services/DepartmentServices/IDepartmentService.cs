using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;

namespace EmployeeAPI.Services.DepartmentServices
{
    public interface IDepartmentService
    {
        public Task<PagedResult<ResponseModel.DepartmentDto>> GetAllAsync(string? name, int? pageIndex, int? pageSize);
        public Task<ResponseModel.CreateDepartment> AddAsync(string Name);
        public Task<ResponseModel.UpdateDepartment> UpdateAsync(Guid id, string Name);
        public Task<string> SoftDeleteAsync(Guid id);
        public Task<PagedResult<UserFilter>> GetStaffByDepartmentAsync(Guid? departmentId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
        public Task<PagedResult<PositionByDepartment>> GetListPositionAsync(Guid? departmentId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
    }
}