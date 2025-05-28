namespace EmployeeAPI.Models
{
    public class AllowedIP
    {
        public Guid AllowedIPId { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public bool isDeleted { get; set; } = false;
    }
}
