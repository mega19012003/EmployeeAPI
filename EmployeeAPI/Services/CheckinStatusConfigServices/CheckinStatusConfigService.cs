using EmployeeAPI.Models;
using EmployeeAPI.Repositories.CheckinStatusConfigs;

namespace EmployeeAPI.Services.CheckinStatusConfigServices
{
    public class CheckinStatusConfigService : ICheckinStatusConfigService
    {
        private readonly ICheckinStatusConfigRepository _repository;
        private readonly AppDbContext _context;

        public CheckinStatusConfigService(ICheckinStatusConfigRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<IEnumerable<CheckinStatusConfig>> GetAllConfigsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<CheckinStatusConfig?> GetConfigAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<CheckinStatusConfig> UpdateConfigAsync(CheckinStatusConfig updatedConfig)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _repository.GetByIdAsync(updatedConfig.Id);
                if (existing == null)
                    throw new Exception("Config not found");

                existing.SalaryMultiplier = updatedConfig.SalaryMultiplier;
                existing.Name = updatedConfig.Name;

                await _repository.UpdateAsync(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CheckinStatusConfig
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
