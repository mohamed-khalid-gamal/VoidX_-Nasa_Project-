using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nasa.Models
{
    public class UserPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsShared { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [ForeignKey("Author")]
        public string UserId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public virtual List<UserFavoritePost> UsersWhoFavorited { get; set; } = new List<UserFavoritePost>(); // Added this line
    }
}
