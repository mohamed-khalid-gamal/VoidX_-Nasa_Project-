using System.ComponentModel.DataAnnotations;

namespace Nasa.DTO
{
    public class AdminPostDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
