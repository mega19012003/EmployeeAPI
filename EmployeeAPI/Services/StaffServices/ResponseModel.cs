using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Services.StaffServices
{
    public static class ResponseModel
    {
        public record StaffDto
        {
            public Guid StaffId { get; set; }
            public string Name { get; set; }
            public DateOnly DateOfBirth { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public Guid DepartmentId { get; set; }
            public string DepartmentName { get; set; }
            public Guid PositionId { get; set; }
            public string PositionName { get; set; }
            public double BasicSalary { get; set; }
            public List<string> ImageUrl { get; set; }
            public bool IsActive { get; set; } = true;
        }
        public record CreateStaff
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public DateOnly DateOfBirth { get; set; }
            [Required]
            public string PhoneNumber { get; set; }
            [Required]
            public string Address { get; set; }
            [Required]
            public Guid DepartmentId { get; set; }
            [Required]
            public Guid PositionId { get; set; }
            [Required]
            public double BasicSalary { get; set; }
            [Required]
            public List<IFormFile> ImageUrl { get; set; }
        }
        public record UpdateStaff
        {
            [Required]
            public Guid Id { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public Guid DepartmentId { get; set; }
            [Required]
            public Guid PositionId { get; set; }
            [Required]
            public double BasicSalary { get; set; }
            [Required]
            public List<IFormFile> ImageUrl { get; set; }
            public bool IsActive { get; set; }
        }
        public record StaffFilter
        {
            public Guid StaffId { get; set; }
            public string Name { get; set; }
            public double BasicSalary { get; set; }
            public List<string> ImageUrl { get; set; }
        }
        /*public record DeleteStaff
        {

            public Guid Id { get; set; }
            public string Name { get; set; }
            public bool IsDeleted { get; set; } = true;

        }*/
    }
}
