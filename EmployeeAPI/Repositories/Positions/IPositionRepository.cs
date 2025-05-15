using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.Positions
{
    public interface IPositionRepository
    {
        /*public Task<IEnumerable<Position>> GetAllAsync();
        public Task<Position> GetByIdAsync(Guid id);
        public Task<Position> AddAsync(string Name);
        public Task<Position> UpdateAsync(Guid id, string Name);
        public Task<Position> SoftDeleteAsync(Guid id);
        public Task<Position> GetAllEmployee(string name);*/
        Task<IEnumerable<Position>> GetAllAsync();
        Task<Position?> GetByIdAsync(Guid id);
        Task<Position> AddAsync(Position position);
        Task<Position?> UpdateAsync(Position position);
        Task<Position?> SoftDeleteAsync(Guid id);
        Task<Position?> GetAllEmployee(string name);

    }
}
