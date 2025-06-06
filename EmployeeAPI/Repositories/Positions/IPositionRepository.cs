using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Positions
{
    public interface IPositionRepository
    {
        IQueryable<Position> GetQueryable();
        Task<Position> GetByIdAsync(Guid id);
        Task AddAsync(Position position);
        Task UpdateAsync(Position position);
        //Task<Position> SoftDeleteAsync(Guid id);
        Task<Position?> GetAllEmployee(string name);
        Task<IEnumerable<Position>> GetStaffByPositionAsync(Guid positionId, int? pageSize, int? pageIndex);
    }
}
