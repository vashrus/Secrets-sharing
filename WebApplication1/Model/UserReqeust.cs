using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class UserReqeust
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
