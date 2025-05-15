using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Duties
{
    public interface IDutyRepository
    {
        Task<IEnumerable<Duty>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm);
        Task<Duty> GetByIdAsync(Guid id);
        Task<Duty> AddAsync(Duty duty);
        Task<Duty> UpdateAsync(Duty duty);
        Task<Duty> SoftDeleteAsync(Guid id);
        Task<IEnumerable<Duty>> GetDutyByName(string name, int? pageSize, int? pageIndex);
        //Task<Duty> GetUnfinishedDuty(string status);
        //Task<Duty> FinishedDuty(string status);
    }
}
