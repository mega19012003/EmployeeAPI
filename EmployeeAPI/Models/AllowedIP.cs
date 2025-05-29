namespace EmployeeAPI.Models
{
    public class AllowedIP
    {
        public Guid AllowedIPId { get; set; }
        public string IPAddress { get; set; } = string.Empty;
    }
}
