using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Nasa.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual List<UserPost> Posts { get; set; } = new List<UserPost>();
        public virtual List<UserFavoritePost> FavoritePosts { get; set; } = new List<UserFavoritePost>(); // Changed to virtual and renamed
        public string ImageUrl { get; set; }
    }
}
