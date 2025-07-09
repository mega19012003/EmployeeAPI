using System.Security.Claims;
using static EmployeeAPI.Services.Dashboards.ResponseModel;

namespace EmployeeAPI.Services.Dashboards
{
    public interface IDashboardService
    {
        Task<DashboardOverviewDto> GetOverviewAsync(ClaimsPrincipal user);
    }
}
