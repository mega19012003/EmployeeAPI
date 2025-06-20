namespace EmployeeAPI.Services.HolidayServices
{
    public class ResponseModel
    {
        public class HolidayResultDto
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }
        public class CreateHolidayDto
        {
            public string Name { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }

        public class UpdateHolidayDto
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }
    }
}
