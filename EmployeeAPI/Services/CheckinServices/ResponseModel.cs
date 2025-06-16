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
        public class CheckinDto 
        {
            public Guid CheckinId { get; set; }
            public Guid userId { get; set; }
            public string Name { get; set; }
            public DateTime CheckinDate { get; set; } = DateTime.Now;
            public DateTime CheckoutDate { get; set; } = DateTime.Now;
            public CheckinStatus CheckinStatus { get; set; }
            public CheckinStatus CheckoutStatus { get; set; }
            public string Checkin { get; set; }
            public string Checkout { get; set; }
            public double SalaryPerDay { get; set; } = 0.0;
            //public string updateBy { get; set; }
            //public DateTime UpdateAt { get; set; }
        }
        public class CreateCheckin
        {
            //public Guid Id { get; set; }
            public Guid? userId { get; set; }
            /*[JsonIgnore]
            public DateTime? CheckinDate { get; set; } = DateTime.Now;
            [JsonIgnore]
            public DateTime? CheckoutDate { get; set; } = DateTime.Now;*/
            public CheckinStatus? CheckinStatus { get; set; }
            /*[JsonIgnore]
            public CheckinStatus? CheckoutStatus { get; set; } */
            //[JsonIgnore]
            //public string IpAddress { get; set; }
        }

        public class CreateCheckout
        {
         
            public Guid? userId { get; set; }
            //[JsonIgnore]
            //public DateTime? CheckoutDate { get; set; } 
            //[JsonIgnore]
            public CheckinStatus? CheckoutStatus { get; set; } 
            //[JsonIgnore]
            //public string IpAddress { get; set; }
        }

        public class UpdateCheckin
        {
            public Guid CheckinId { get; set; }
            //public Guid userId { get; set; }
            //public DateTime CheckinDate { get; set; }
            public CheckinStatus CheckinStatus { get; set; }
            public CheckinStatus CheckoutStatus { get; set; }
            //public string Status { get; set; }
        }

    }
}
