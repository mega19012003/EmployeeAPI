using System.Linq.Expressions;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;


namespace EmployeeAPI.Services.CheckinServices
{
    public interface ICheckinService
    {
        Task<PagedResult<ResponseModel.CheckinResultDto>> GetAllAsync(string? StaffName, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRoles);
        //Task<ResponseModel.CheckinResultDto> GetByIdAsync(Guid id);
        Task AutoMarkAbsentAsync(TimeOnly EndTimeAfternoon);
        Task<ResponseModel.CheckinResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        //Task<ResponseModel.CheckinResultDto> CreateCheckinAsync(ResponseModel.CreateCheckinDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.CheckinResultDto> CheckinAsync(Guid? userId, Guid currentUserId, IList<string> roles);
        Task<ResponseModel.CheckinResultDto> CheckoutAsync(Guid? userId, Guid currentUserId, IList<string> roles);
        //Task<ResponseModel.CheckinResultDto> CreateCheckinAfternoonAsync(ResponseModel.CreateCheckinDto dto, Guid currentUserId, IList<string> currentUserRoles);
        //Task<ResponseModel.CheckinResultDto> CheckoutAsync(ResponseModel. CreateCheckoutDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.CheckinResultDto> UpdateAsync(ResponseModel.UpdateCheckinDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<PagedResult<ResponseModel.CheckinResultDto>> GetCheckinByUserAsync(Guid currentUserId, IList<string> currentUserRoles, Guid? staffId, int? pageIndex, int? pageSize);
    }
}
