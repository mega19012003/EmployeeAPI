using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.PayrollServices
{
    public class ResponseModel
    {
        public class PayrollDto //: BaseDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public double Salary { get; set; }
            public DateTime CreatedDate { get; set; }
            //public DateTime CheckoutDate { get; set; } //ngày trả lương
            public string Note { get; set; }
            public bool IsDeleted { get; set; } = false;
            public int DaysWorked { get; set; }
            //public bool IsPaid { get; set; } = false; //đã trả lương
        }

        public class CreatePayroll
        {
            public Guid UserId { get; set; }
            public string Note { get; set; }
            public DateTime CreatedDate { get; set; } 
        }

        public class UpdatePayroll
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string Note { get; set; }
        }

        public class PaidPayroll
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
