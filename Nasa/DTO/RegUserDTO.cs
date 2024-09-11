using System.ComponentModel.DataAnnotations;

namespace Nasa.DTO
{
    public class RegUserDTO
    {
        [Required]
        
        public string UserName { get; set; }
        [Required]
        public string PassWord { get; set; }
        [Compare("PassWord")]
        [Required]
        public string ConfirmPassword { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }
}
