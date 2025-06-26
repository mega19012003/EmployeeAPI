using EmployeeAPI.Repositories.ScheduleTimes;

namespace EmployeeAPI.Services.CheckinServices
{
    public class AbsentBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AbsentBackgroundService> _logger;

        public AbsentBackgroundService(IServiceProvider serviceProvider, ILogger<AbsentBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    using var scope = _serviceProvider.CreateScope();
                    var scheduleRepo = scope.ServiceProvider.GetRequiredService<IScheduleTimeRepository>();
                    var checkinService = scope.ServiceProvider.GetRequiredService<ICheckinService>();

                    var schedule = await scheduleRepo.GetScheduleTime();

                    if (schedule != null)
                    {
                        var EndTimeCheck = schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime).AddMinutes(schedule.LateThresholdMinutes);
                        var nowTimeOnly = TimeOnly.FromDateTime(vnTime);

                        _logger.LogInformation($"[BackgroundService] Bây giờ là {nowTimeOnly}, giờ kết thúc ca là {EndTimeCheck}");

                        // Gọi AutoMarkAbsentAsync nếu giờ hiện tại >= EndTimeAfternoon (để đảm bảo không bị bỏ lỡ)
                        if (nowTimeOnly >= EndTimeCheck)
                        {
                            await checkinService.AutoMarkAbsentAsync(EndTimeCheck);
                        }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi background: " + ex.Message);
                }
            }
        }
    }
}
