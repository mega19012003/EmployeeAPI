using EmployeeAPI.Base;
using EmployeeAPI.Services.HolidayServices;

namespace EmployeeAPI.Services.HolidayServices
{
    public interface IHolidayService
    {
        Task<PagedResult<ResponseModel.HolidayDto>> GetAllAsync(string? name, int? pageSize, int? pageIndex);
        Task<ResponseModel.HolidayDto> GetByIdAsync(Guid id);
        Task<ResponseModel.HolidayDto> CreateAsync(ResponseModel.CreateHoliday dto);
        Task<ResponseModel.HolidayDto> UpdateAsync(ResponseModel.UpdateHoliday dto);
        Task<string> DeleteAsync(Guid id);
    }
}
