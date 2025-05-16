using EmployeeAPI.Models;

namespace EmployeeAPI.Services.PayrollServices
{
    public class ResponseModel
    {
        public record PayrollDto
        {
            public Guid Id { get; set; }
            public Guid StaffId { get; set; }
            public string StaffName { get; set; }
            public double Salary { get; set; }
            public DateTime CreatedDate { get; set; }
            //public DateTime CheckoutDate { get; set; } //ngày trả lương
            public string Note { get; set; }
            public bool IsDeleted { get; set; } = false;
            //public bool IsPaid { get; set; } = false; //đã trả lương
        }

        public record CreatePayroll
        {
            public Guid StaffId { get; set; }
            public string Note { get; set; }
            public DateTime CreatedDate { get; set; } 
        }

        public record UpdatePayroll
        {
            public Guid Id { get; set; }
            public Guid StaffId { get; set; }
            public string Note { get; set; }
        }

        public record PaidPayroll
        {
            public Guid Id { get; set; }
            public Guid StaffId { get; set; }
            public double Salary { get; set; }
            public DateTime CheckoutDate { get; set; } //ngày trả lương
            public string Note { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool isPaid { get; set; }
        }
    }
}
