using EmployeeAPI.Models;
using EmployeeAPI.Repositories.CheckinStatusConfigs;

namespace EmployeeAPI.Services.CheckinStatusConfigServices
{
    public class CheckinStatusConfigService : ICheckinStatusConfigService
    {
        private readonly ICheckinStatusConfigRepository _repository;

        public CheckinStatusConfigService(ICheckinStatusConfigRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CheckinStatusConfig>> GetAllConfigsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<CheckinStatusConfig?> GetConfigAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<CheckinStatusConfig> UpdateConfigAsync(CheckinStatusConfig updatedConfig)
        {
            var existing = await _repository.GetByIdAsync(updatedConfig.Id);
            if (existing == null)
                throw new Exception("Config not found");

            existing.SalaryMultiplier = updatedConfig.SalaryMultiplier;
            existing.Name = updatedConfig.Name;

            await _repository.UpdateAsync(existing);

            return new CheckinStatusConfig
            {
                Id = existing.Id,
                Name = existing.Name,
                SalaryMultiplier = existing.SalaryMultiplier,
                Note = existing.Note
            };
        }
    }
}
