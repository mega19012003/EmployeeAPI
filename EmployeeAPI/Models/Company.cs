namespace EmployeeAPI.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } 
        public string? Address { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        //public User? Founder { get; set; }
        //public string? FounderName { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
