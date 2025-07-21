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

        public class UserWithPayrollDto
        {
            public Guid UserId { get; set; }
            public string Fullname { get; set; } = string.Empty;
            public string CompanyName { get; set; }
            public string DepartmentName { get; set; }
            public string PositionName { get; set; }
            public List<PayrollResultDto> Payrolls { get; set; } = new();
        }

    }
}
