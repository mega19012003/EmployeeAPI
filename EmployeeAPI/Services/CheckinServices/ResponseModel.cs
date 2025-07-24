using System;
using System.Text.Json.Serialization;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;

namespace EmployeeAPI.Services.CheckinServices
{
    public class ResponseModel
    {
        public class CheckinResultDto
        {
            public Guid CheckinId { get; set; }
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public DateTime CheckinTime { get; set; }
            public DateTime CheckoutTime { get; set; }
            public string Status { get; set; }
            public int? LogStatus { get; set; }
        }

        public class CreateCheckinDto
        {
            public Guid? userId { get; set; }
            //public Enums.LogStatus? LogStatus { get; set; }
            //public DateTime CheckinTime { get; set; } = DateTime.Now;
        }

        public class CreateCheckoutDto
        {
         
            public Guid? userId { get; set; }
            //public Enums.LogStatus? CheckoutAfternoonStatus { get; set; } 
            //public DateTime CheckoutTime { get; set; } = DateTime.Now;
        }

        public class UpdateCheckinDto
        {
            public Guid CheckinId { get; set; }
            public Enums.LogStatus LogStatus { get; set; }
        }

        public class CheckinDetailDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime CheckinTime { get; set; }
            public DateTime CheckoutTime { get; set; }
            public int? LogStatus { get; set; }
            public string Status { get; set; }
        }

        public class UserWithCheckinsDto
        {
            public Guid UserId { get; set; }
            public string FullName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string ImageUrl { get; set; }

            public List<CheckinResultDto> Checkins { get; set; } = new();
        }
    }
}
