using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Nasa.Models
{
    public class AdminPost
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
