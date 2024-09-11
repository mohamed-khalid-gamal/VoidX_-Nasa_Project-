using System.ComponentModel.DataAnnotations;

namespace Nasa.DTO
{
    public class MessageDTO
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
