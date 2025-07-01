//using EmployeeAPI.Repositories.ScheduleTimes;

//namespace EmployeeAPI.Services.CheckinServices
//{
//    public class AbsentBackgroundService : BackgroundService
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly ILogger<AbsentBackgroundService> _logger;

//        public AbsentBackgroundService(IServiceProvider serviceProvider, ILogger<AbsentBackgroundService> logger)
//        {
//            _serviceProvider = serviceProvider;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

//                    using var scope = _serviceProvider.CreateScope();
//                    var scheduleRepo = scope.ServiceProvider.GetRequiredService<IScheduleTimeRepository>();
//                    var checkinService = scope.ServiceProvider.GetRequiredService<ICheckinService>();

//                    var schedule = await scheduleRepo.GetScheduleTime();

//                    if (schedule != null)
//                    {
//                        var EndTimeCheck = schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime).AddMinutes(schedule.LateThresholdMinutes);
//                        var nowTimeOnly = TimeOnly.FromDateTime(vnTime);

//                        _logger.LogInformation($"[BackgroundService] Bây giờ là {nowTimeOnly}, giờ kết thúc ca là {EndTimeCheck}");

//                        // Gọi AutoMarkAbsentAsync nếu giờ hiện tại >= EndTimeAfternoon (để đảm bảo không bị bỏ lỡ)
//                        if (nowTimeOnly >= EndTimeCheck)
//                        {
//                            await checkinService.AutoMarkAbsentAsync(EndTimeCheck);
//                        }
//                    }
//                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine("Lỗi background: " + ex.Message);
//                }
//            }
//        }
//    }
//}















//public async Task AutoMarkAbsentAsync(TimeOnly CheckTime)
//{
//    var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
//    var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
//    var today = vnNow.Date;

//    if (vnNow.DayOfWeek == DayOfWeek.Sunday)
//    {
//        _logger.LogInformation("Sunday, no checkin");
//        return;
//    }

//    bool isHoliday = await _holidayRepository.IsHolidayAsync(today);
//    if (isHoliday)
//    {
//        _logger.LogInformation("today is a holiday, no marking absent");
//        return;
//    }

//    if (TimeOnly.FromDateTime(vnNow) < CheckTime)
//    {
//        _logger.LogInformation("Not work end time, no marking absent");
//        return;
//    }

//    var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
//    if (schedule == null) throw new Exception("Work schedule time hasn't been set");

//    var currentTimeOnly = TimeOnly.FromDateTime(vnNow);
//    //var overtimeThreshold = schedule.EndTimeAfternoon.AddMinutes(schedule.LogAllowtime).AddMinutes(schedule.LateThresholdMinutes);

//    TimeSpan OvertimeDuration = currentTimeOnly - schedule.EndTimeAfternoon;

//    var vnTodayStart = vnNow.Date; 
//    var vnTodayStartUtc = TimeZoneInfo.ConvertTimeToUtc(vnTodayStart, vnTimeZone);
//    var vnTodayEndUtc = vnTodayStartUtc.AddDays(1);

//    var checkinsInRange = await _context.Checkins
//        .Where(c => c.CheckinMorning >= vnTodayStartUtc && c.CheckinMorning < vnTodayEndUtc)
//        .ToListAsync();

//    var checkedInUserIds = checkinsInRange
//        .Where(c => TimeZoneInfo.ConvertTimeFromUtc(c.CheckinMorning, vnTimeZone).Date == today)
//        .Select(c => c.UserId).Distinct().ToList();

//    var allUsers = await _userRepository.GetAll().ToListAsync();

//    var absentUsers = allUsers.Where(u => !checkedInUserIds.Contains(u.UserId)).ToList();

//    foreach (var user in absentUsers)
//    {

//        var checkin = new Checkin
//        {
//            Id = Guid.NewGuid(),
//            UserId = user.UserId,
//            CheckinMorningStatus = Enums.LogStatus.Absent,
//            CheckinMorning = DateTime.UtcNow,
//            CheckoutMorningStatus = Enums.LogStatus.Absent,
//            CheckoutMorning = DateTime.UtcNow,
//            CheckinAfternoonStatus = Enums.LogStatus.Absent,
//            CheckinAfternoon = DateTime.UtcNow,
//            CheckoutAfternoonStatus = Enums.LogStatus.Absent,
//            CheckoutAfternoon = DateTime.UtcNow,
//        };

//        await _checkinRepository.CreateAsync(checkin);
//    }

//    await _context.SaveChangesAsync();

//    _logger.LogInformation($"Mark {absentUsers.Count} users absent on {today:dd/MM/yyyy}.");
//}
