using EmployeeAPI.Base;
using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CompanyServices.ResponseModel;

namespace EmployeeAPI.Services.CompanyServices
{
    public interface ICompanyService
    {
        Task<CompanyResultDto> GetCompanyByIdAsync(Guid companyId, Guid currentUserId, IList<string> curretnUserRole);
        Task<PagedResult<CompanyResultDto>> GetAllCompaniesAsync(string? Name, int? pageIndex, int? pagesize);
        Task<CompanyResultDto> CreateCompanyAsync(CreateCompanyDto dto);
        Task<CompanyResultDto> UpdateCompanyAsync(UpdateCompanyDto dto);
        Task<string> DeleteCompanyAsync(Guid companyId);
    }
}
