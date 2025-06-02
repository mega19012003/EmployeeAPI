namespace EmployeeAPI.Services.CheckinServices
{
    public class AbsentBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AbsentBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    if (vnTime.Hour == 18 && vnTime.Minute == 0)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var checkinService = scope.ServiceProvider.GetRequiredService<ICheckinService>();
                        await checkinService.AutoMarkAbsentAsync();
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
