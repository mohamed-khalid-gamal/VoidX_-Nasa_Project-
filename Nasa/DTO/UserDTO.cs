using System.ComponentModel.DataAnnotations;

namespace Nasa.DTO
{
    public class UserDTO
    {
        public string UserName { get; set; }
        [Phone]
        public string Phone { get; set; }
       // public string ImageUrl { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        

    }
}
