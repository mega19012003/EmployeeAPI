using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Duties
{
    public interface IDutyRepository
    {
        Task<IEnumerable<Duty>> GetAllAsync();
        IQueryable<Duty> GetAllQueryable();
        Task<Duty> GetDutyByIdAsync(Guid id);
        Task<DutyDetail> GetDutyDetailByIdAsync(Guid id);
        Task<Duty> AddAsync(Duty duty);
        Task<DutyDetail> AddDutyDetailAsync(DutyDetail detail);
        Task UpdateDutyAsync(Duty duty);
        Task UpdateDutyDetailAsync(DutyDetail duty);
        //Task<Duty> SoftDeleteDutyAsync(Guid id);
        //Task<DutyDetail> SoftDeleteDutyDetailAsync(Guid id);
        Task<IEnumerable<Duty>> GetDutyByName();
    }
}
