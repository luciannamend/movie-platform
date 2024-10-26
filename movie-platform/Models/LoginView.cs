using System.ComponentModel.DataAnnotations;

namespace movie_platform.Models
{
    public class LoginView
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
