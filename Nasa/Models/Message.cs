using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Nasa.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
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
