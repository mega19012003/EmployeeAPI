namespace EmployeeAPI.Services.HolidayServices
{
    public class ResponseModel
    {
        public class HolidayDto
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
        public class CreateHoliday
        {
            public string Name { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }

        public class UpdateHoliday
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }
    }
}
