using System.ComponentModel.DataAnnotations;

namespace Nasa.DTO
{
    public class AddUserPostDTO
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public string Content { get; set; }
        [Required]
        public IFormFile image { get; set; }
    }
}
