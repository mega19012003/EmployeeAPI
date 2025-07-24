using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.PayrollServices
{
    public class ResponseModel
    {
        public class PayrollResultDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
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

        public class UserWithPayrollDto
        {
            public Guid UserId { get; set; }
            public string Fullname { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string ImageUrl { get; set; }
            public List<PayrollResultDto> Payrolls { get; set; } = new();
        }

    }
}
