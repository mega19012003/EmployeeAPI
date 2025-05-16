using System.Linq.Expressions;
using EmployeeAPI.Models;


namespace EmployeeAPI.Services.CheckinServices
{
    public interface ICheckinService
    {
        Task<IEnumerable<ResponseModel.CheckinDto>> GetAllAsync();
        Task<ResponseModel.CheckinDto> GetByIdAsync(Guid id);
        Task<ResponseModel.CheckinDto> CreateAsync(ResponseModel.CreateCheckin dto);
        Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto);
        Task<string> DeleteAsync(Guid id);
        //Task<bool> ExistsAsync(Expression<Func<Checkin, bool>> predicate);
    }
}
