using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Companies
{
    public interface ICompanyRepository
    {
        Task<Company> GetCompanyByIdAsync(Guid id);
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<Company> AddCompanyAsync(Company company);
        Task<Company> UpdateCompanyAsync(Company company);
        Task<bool> HasUsersUsingCompanyAsync(Guid departmentId);
    }
}
