using EmployeeAPI.Models;
using EmployeeAPI.Repositories.LogStatusConfigs;

namespace EmployeeAPI.Services.LogStatusConfigservices
{
    public class LogStatusConfigService : ILogStatusConfigService
    {
        private readonly ILogStatusConfigRepository _repository;
        private readonly AppDbContext _context;

        public LogStatusConfigService(ILogStatusConfigRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<IEnumerable<LogStatusConfig>> GetAllConfigsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<LogStatusConfig?> GetConfigAsync(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                throw new ArgumentException("Config not found");
            return new LogStatusConfig
            {
                Id = result.Id,
                Name = result.Name,
                SalaryMultiplier = result.SalaryMultiplier,
                Note = result.Note
            };
        }

        public async Task<LogStatusConfig> UpdateConfigAsync(LogStatusConfig updatedConfig)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _repository.GetByIdAsync(updatedConfig.Id);
                if (existing == null)
                    throw new ArgumentException("Config not found");

                existing.SalaryMultiplier = updatedConfig.SalaryMultiplier;
                existing.Name = updatedConfig.Name;

                await _repository.UpdateAsync(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new LogStatusConfig
                {
                    Id = existing.Id,
                    Name = existing.Name,
                    SalaryMultiplier = existing.SalaryMultiplier,
                    Note = existing.Note
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ArgumentException("Invalid input data for checkin status config update. Please check the provided values and try again.", ex);
            }
        }
    }
}
