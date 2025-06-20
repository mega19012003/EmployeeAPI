using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.PayrollServices
{
    public class ResponseModel
    {
        public class PayrollResultDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public double Salary { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Note { get; set; }
            public int DaysWorked { get; set; }
        }
        public class CreatePayrollDto
        {
            public Guid UserId { get; set; }
            public string Note { get; set; }
            public DateTime CreatedDate { get; set; } 
        }

        public class UpdatePayrollDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string Note { get; set; }
        }

        public class PaidPayrollDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public double Salary { get; set; }
            public string Note { get; set; }
            public DateTime CreatedDate { get; set; }
            //public bool isPaid { get; set; }
            public int DaysWorked { get; set; } //ngày công
        }
    }
}
