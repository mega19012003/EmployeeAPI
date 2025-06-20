using EmployeeAPI.Base;
using EmployeeAPI.Services.HolidayServices;

namespace EmployeeAPI.Services.HolidayServices
{
    public interface IHolidayService
    {
        Task<PagedResult<ResponseModel.HolidayResultDto>> GetAllAsync(string? name, int? pageSize, int? pageIndex);
        Task<ResponseModel.HolidayResultDto> GetByIdAsync(Guid id);
        Task<ResponseModel.HolidayResultDto> CreateAsync(ResponseModel.CreateHolidayDto dto);
        Task<ResponseModel.HolidayResultDto> UpdateAsync(ResponseModel.UpdateHolidayDto dto);
        Task<string> DeleteAsync(Guid id);
    }
}
