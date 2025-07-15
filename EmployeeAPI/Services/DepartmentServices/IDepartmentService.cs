using EmployeeAPI.Base;
using EmployeeAPI.Models;
using System;
using System.Security.Claims;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;

namespace EmployeeAPI.Services.DepartmentServices
{
    public interface IDepartmentService
    {
        public Task<PagedResult<ResponseModel.DepartmentResultDto>> GetAllAsync(string? name, Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles);
        public Task<ResponseModel.DepartmentResultDto> GetByIdAsync(Guid id);
        public Task<ResponseModel.DepartmentResultDto> AddAsync(string name, Guid currentUserId, IList<string> currentUserRole);
        public Task<ResponseModel.DepartmentResultDto> UpdateAsync(Guid id, string Name, Guid currentUserId, IList<string> curretnUserRole);
        public Task<string> SoftDeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRole);
        //public Task<PagedResult<UserFilterDto>> GetStaffByDepartmentAsync(Guid? departmentId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
        //public Task<PagedResult<ResponseModel.PositionByDepartmentDto>> GetListPositionAsync(Guid? departmentId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
    }
}