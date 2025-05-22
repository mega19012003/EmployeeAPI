using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
            [JsonIgnore]
            public Guid Id { get; set; }
            [Required]
            public string Username { get; set; }
            [Required]
            public string Password { get; set; }
            [Required]
            public string Fullname { get; set; }
        }

        public class TokenDto
        {
            public string token;
        }

    }
}
