using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;

namespace EmployeeAPI.Services.DutyServices
{
    public interface IDutyService
    {
        Task<PagedResult<ResponseModel.DutyDto>> GetAllAsync(string? SearchTerm, int? pageSize, int? pageIndex);
        Task<ResponseModel.DutyDto> GetByIdAsync(Guid id);
        Task<ResponseModel.CreateDuty> AddAsync(ResponseModel.CreateDuty dto);
        Task<ResponseModel.UpdateDuty> UpdateAsync(ResponseModel.UpdateDuty dto);
        Task<string> SoftDeleteAsync(Guid id);
        //WTask<ResponseModel.DutyDto> GetDutyByName(string name);
        //Task<ResponseModel.DutyDto> GetUnfinishedDuty(string status);
    }
}
