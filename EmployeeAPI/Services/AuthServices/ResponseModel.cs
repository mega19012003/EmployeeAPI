using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Services.AuthServices
{
    public class ResponseModel
    {
        public class LoginDto
        {
            [Required]
            public string Username { get; set; }
            [Required]
            public string Password { get; set; }
        }
        public class RegisterDto
        {
            [Required]
            public string Username { get; set; }
            [Required]
            public string Password { get; set; }
            [Required]
            public string Fullname { get; set; }
        }
    }
}
