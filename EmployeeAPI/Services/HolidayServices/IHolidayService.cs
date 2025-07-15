using EmployeeAPI.Base;
using EmployeeAPI.Services.HolidayServices;

namespace EmployeeAPI.Services.HolidayServices
{
    public interface IHolidayService
    {
        Task<PagedResult<ResponseModel.HolidayResultDto>> GetAllAsync(string? name, Guid? companyId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.HolidayResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.HolidayResultDto> CreateAsync(ResponseModel.CreateHolidayDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.HolidayResultDto> UpdateAsync(ResponseModel.UpdateHolidayDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
    }
}
