using System.Linq.Expressions;
using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Checkins
{
    public interface ICheckinRepository
    {
        Task<IEnumerable<Checkin>> GetAllAsync();
        Task<Checkin> GetByIdAsync(Guid id);
        Task CreateAsync(Checkin checkin);
        Task UpdateAsync(Checkin checkin);
        Task<Checkin> SoftDeleteAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<Checkin, bool>> predicate);
   
    }
}
