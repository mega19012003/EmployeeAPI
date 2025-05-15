using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;

namespace EmployeeAPI.Services.DutyServices
{
    public interface IDutyService
    {
        Task<IEnumerable<ResponseModel.DutyDto>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm);
        Task<ResponseModel.DutyDto> GetByIdAsync(Guid id);
        Task<ResponseModel.CreateDuty> AddAsync(ResponseModel.CreateDuty dto);
        Task<ResponseModel.UpdateDuty> UpdateAsync(ResponseModel.UpdateDuty dto);
        Task<ResponseModel.DeleteDuty> SoftDeleteAsync(Guid id);
        //WTask<ResponseModel.DutyDto> GetDutyByName(string name);
        //Task<ResponseModel.DutyDto> GetUnfinishedDuty(string status);
    }
}
