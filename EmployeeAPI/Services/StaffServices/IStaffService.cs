using EmployeeAPI.Models;

namespace EmployeeAPI.Services.StaffServices
{
    public interface IStaffService
    {
        Task<IEnumerable<ResponseModel.StaffDto>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm);
        Task<ResponseModel.StaffDto> GetByIdAsync(Guid id);
        Task<ResponseModel.StaffDto> AddAsync(ResponseModel.CreateStaff dto);
        Task<ResponseModel.StaffDto> UpdateAsync(ResponseModel.UpdateStaff staff);
        Task<string> SoftDeleteAsync(Guid staff);
        Task<IEnumerable<ResponseModel.StaffDto>> GetByNameAsync(string name, int? pageSize, int? pageIndex);
        Task<IEnumerable<ResponseModel.StaffDto>> GetEmployeeByPosition(string SearchTerm, int? pageSize, int? pageIndex); // nhớ sửa
    }
}
