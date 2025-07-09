namespace EmployeeAPI.Services.Dashboards
{
    public class ResponseModel
    {
        public class DashboardOverviewDto
        {
            public int TotalEmployees { get; set; }
            public int ActiveEmployees { get; set; }
            public int TotalDepartments { get; set; }
            public int TotalPositions { get; set; }
            //public int CheckinLateCountToday { get; set; }
            public int TotalCheckinsToday { get; set; }
            public decimal TotalPayrollThisMonth { get; set; }
            public List<UpcomingHolidayDto> UpcomingHolidays { get; set; }
        }

        public class UpcomingHolidayDto
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
