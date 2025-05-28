namespace EmployeeAPI.Repositories.AllowedIPs
{
    public class ResponseModel
    {
        public class IPDto 
        {
            public Guid AllowedIPId { get; set; }
            public string IPAddress { get; set; } = string.Empty;
        }

        /*public class updateIPDto
        {
            public Guid AllowedIPId { get; set; }
            public string IPAddress { get; set; } = string.Empty;
        }*/
    }
}
