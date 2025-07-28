using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Holidays
{
    public interface IHolidayRepository
    {
        IQueryable<Holiday> GetAll();
        Task<Holiday> GetByIdAsync(Guid id);
        Task<Holiday> CreateAsync(Holiday holiday);
        Task<Holiday> UpdateAsync(Holiday holiday);
        Task<Holiday> SoftDeleteAsync(Holiday holiday);
        Task<bool> IsHolidayAsync(DateTime date, Guid companyId);
    }
}
