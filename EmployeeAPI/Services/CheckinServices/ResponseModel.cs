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
            public string Name { get; set; }
            public DateTime CheckinDate { get; set; } = DateTime.Now;
            public DateTime CheckoutDate { get; set; } = DateTime.Now;
            public string Checkin { get; set; }
            public string Checkout { get; set; }
            public double SalaryPerDay { get; set; } = 0.0;
        }

        public class CreateCheckinDto
        {
            public Guid? userId { get; set; }
            //public Enums.LogStatus? LogStatus { get; set; }

        }

        public class CreateCheckoutDto
        {
         
            public Guid? userId { get; set; }
            //public Enums.LogStatus? CheckoutStatus { get; set; } 
        }

        public class UpdateCheckinDto
        {
            public Guid CheckinId { get; set; }
            public Enums.LogStatus CheckinStatus { get; set; }
            public Enums.LogStatus CheckoutStatus { get; set; }
        }
    }
}
