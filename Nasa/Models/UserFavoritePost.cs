using System.ComponentModel.DataAnnotations.Schema;

namespace Nasa.Models
{
    public class UserFavoritePost
    {
        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        public virtual UserPost Post { get; set; }
        public virtual ApplicationUser Author { get; set; } // Renamed to Author to reflect the user owning the relationship
    }
}
