using System.Linq.Expressions;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;


namespace EmployeeAPI.Services.CheckinServices
{
    public interface ICheckinService
    {
        Task<PagedResult<ResponseModel.CheckinDto>> GetAllAsync(string? StaffName, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.CheckinDto> GetByIdAsync(Guid id);
        Task<ResponseModel.CheckinDto> CreateAsync(ResponseModel.CreateCheckin dto);
        Task<ResponseModel.CheckinDto> CheckoutAsync(ResponseModel.CreateCheckout dto);
        Task AutoMarkAbsentAsync();
        Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto, Guid currentUserId, IList<string> currentUserRoles);
        //Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto);
        Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        //Task<string> DeleteAsync(Guid id);
        //Task<bool> ExistsAsync(Expression<Func<Checkin, bool>> predicate);
        //Task<PagedResult<CheckinDto>> GetCheckinByUserAsync(Guid staffId, int? pageIndex, int? pageSize);
        Task<PagedResult<ResponseModel.CheckinDto>> GetCheckinByUserAsync(Guid currentUserId, IList<string> currentUserRoles, Guid? staffId, int? pageIndex, int? pageSize);
    }
}
