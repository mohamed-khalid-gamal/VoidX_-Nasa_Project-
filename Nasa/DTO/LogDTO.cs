using System.ComponentModel.DataAnnotations;

namespace Nasa.DTO
{
    public class LogDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
