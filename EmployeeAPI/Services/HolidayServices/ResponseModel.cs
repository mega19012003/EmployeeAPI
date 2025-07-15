namespace EmployeeAPI.Services.HolidayServices
{
    public class ResponseModel
    {
        public class HolidayResultDto
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateOnly startDate { get; set; }
            public DateOnly endDate { get; set; }
            public string companyName { get; set; }
        }
        public class CreateHolidayDto
        {
            public string Name { get; set; }
            public DateOnly startDate { get; set; }
            public DateOnly endDate { get; set; }
        }

        public class UpdateHolidayDto
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateOnly startDate { get; set; }
            public DateOnly endDate { get; set; }
        }
    }
}
