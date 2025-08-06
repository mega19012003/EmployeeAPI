using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.AllowedIPs
{
    public interface IAllowedIPRepository
    {
        IQueryable<AllowedIP> GetAll();
        Task<IEnumerable<AllowedIP>> GetAllAsync();
        Task<AllowedIP> GetByIdAsync(Guid id);
        Task<List<AllowedIP>> GetByCompanyIdAsync(Guid companyId);
        Task AddAsync(AllowedIP entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string ip, Guid companyId);
    }
}
