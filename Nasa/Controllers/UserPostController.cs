using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nasa.Data;
using Nasa.DTO;
using Nasa.Models;

namespace Nasa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPostController(IWebHostEnvironment _webHost , UserManager<ApplicationUser> _userManager , ApplicationDbContext _db) : ControllerBase
    {
        [HttpPost("SavePost")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SavePost([FromForm] AddUserPostDTO userPost)
        {
            if (ModelState.IsValid)
            {
                if (_db.posts.FirstOrDefault(e=>e.Title == userPost.Title) != null)
                {
                    ModelState.AddModelError("title", "Title Must be Unique");
                    return BadRequest(ModelState);
                }

            if (userPost.image == null) { return BadRequest("there is no Image"); }
            var ImageUrl = await SaveImageAsync(userPost.image);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(userPost.image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
                }

                // Validate MIME type for additional security (optional)
                var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedMimeTypes.Contains(userPost.image.ContentType.ToLower()))
                {
                    return BadRequest("Invalid image file type.");
                }

                var post = new UserPost()
            {
                Content =userPost.Content,
                Title = userPost.Title,
                ImageUrl = ImageUrl,
                Date = DateTime.Now,
                UserId = _userManager.GetUserId(User),
                Author = await _userManager.GetUserAsync(User),
                IsShared = false
            };
            _db.posts.Add(post);
            await _db.SaveChangesAsync();
            var user = await _userManager.GetUserAsync(User);
            user.Posts.Add(post);
            await _db.SaveChangesAsync();
            return Ok("Post Saved Succefully Without Share");
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        [HttpPost("IsSaved/id={id:int}")]
        public async Task<IActionResult> IsSaved(int id)
        {
            if (_db.posts.FirstOrDefault(e => e.Id == id) != null)
            {
                return Ok("True");
            }
            else
            {
                return Ok("False");
            }
        }
        [HttpPost("ShareAsSaved/id={id:int}")]
        public async Task<IActionResult> ShareAsSaved(int id)
        {
            if (id <= 0)
            {
                return BadRequest("invalid Id");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var post = _db.posts.FirstOrDefault(e => e.Id == id);
                    if (post != null) {
                    post.IsShared= true;
                        await _db.SaveChangesAsync();
                        return Ok("Shared Succesfully");
                    }
                    else
                    {
                        return NotFound();  
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
        }
        [HttpPost("ShareAsNotSaved")]
        public async Task<IActionResult> ShareAsNotSaved([FromForm]AddUserPostDTO userPost)
        {
            if (ModelState.IsValid)
            {
                if (_db.posts.FirstOrDefault(e => e.Title == userPost.Title) != null)
                {
                    ModelState.AddModelError("title", "Title Must be Unique");
                    return BadRequest(ModelState);
                }
                if (userPost.image == null) { return BadRequest("there is no Image"); }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(userPost.image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
                }

                // Validate MIME type for additional security (optional)
                var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedMimeTypes.Contains(userPost.image.ContentType.ToLower()))
                {
                    return BadRequest("Invalid image file type.");
                }

                var ImageUrl = await SaveImageAsync(userPost.image);
                var post = new UserPost()
                {
                    Content = userPost.Content,
                    Title = userPost.Title,
                    ImageUrl = ImageUrl,
                    Date = DateTime.Now,
                    UserId = _userManager.GetUserId(User),
                    Author = await _userManager.GetUserAsync(User),
                    IsShared = true
                };
                _db.posts.Add(post);
                await _db.SaveChangesAsync();
                var user = await _userManager.GetUserAsync(User);
                user.Posts.Add(post);
                await _db.SaveChangesAsync();
                return Ok("Post Saved Succefully Without Share");
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        [HttpGet("GetAllPostsPerUser")]
        public async Task<IActionResult> GetAllPostsPerUser()
        {
            // Retrieve the current user along with their posts
            var user = await _userManager.Users
                                         .Include(u => u.Posts) // Eagerly load the Posts
                                         .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map the posts to the DTO
            var posts = user.Posts.Select(e => new UserPostDTO
            {
                UserEmail = e.Author.Email,
                UserPhoto = e.Author.ImageUrl,
                Content = e.Content,
                Date = e.Date,
                Id = e.Id,
                Title = e.Title,
                UserName = user.UserName,
                ImageUrl = e.ImageUrl
            }).ToList();

            return Ok(posts);
        }

        [HttpGet("GetAllSharedPosts")]
        public async Task<IActionResult> GetAllSharedPosts()
        {
            var user = await _userManager.GetUserAsync(User);
            var posts = _db.posts.Where( e => e.IsShared == true && e.UserId != user.Id).Select(e => new UserPostDTO { 
                UserEmail = e.Author.Email,
            Title = e.Title,
            Content = e.Content,
            Id = e.Id,
            UserPhoto = e.Author.ImageUrl,
            ImageUrl = e.ImageUrl,
            UserName = e.Author.UserName,
            Date = e.Date,
            }).ToList();
            return Ok(posts);
        }
        [HttpPost("AddToFavorite/{postId:int}")]
        public async Task<IActionResult> AddToFavorite(int postId)
        {
            var user = await _userManager.Users
                                        .Include(u => u.FavoritePosts) // Eagerly load the Posts
                                        .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));
            var post = _db.posts.Where(e=>e.UserId!=user.Id && e.IsShared == true).FirstOrDefault(e=>e.Id==postId);

            if (post == null)
            {
                return NotFound("Post not found Or you try to add your own post to favorities");
            }

            if (user.FavoritePosts.Any(fp => fp.PostId == postId))
            {
                return BadRequest("Post already in favorites");
            }

            var favorite = new UserFavoritePost
            {
                AuthorId = user.Id,
                PostId = postId,
                Post = post,
                Author = user
            };

            user.FavoritePosts.Add(favorite);
            await _db.SaveChangesAsync();

            return Ok("Post added to favorites");
        }
        [HttpGet("GetUserFavorites")]
        public async Task<IActionResult> GetUserFavorites()
        {
            // Eagerly load the user's FavoritePosts and the related Post entity
            var user = await _userManager.Users
                                         .Include(u => u.FavoritePosts) // Load the FavoritePosts collection
                                         .ThenInclude(fp => fp.Post) // Load each related Post for the FavoritePosts
                                         .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map the favorite posts to the DTO
            var favorites = user.FavoritePosts.Select(fp => new UserPostDTO
            {
                UserEmail = fp.Author.Email,
                UserPhoto = fp.Author.ImageUrl,
                Content = fp.Post.Content,
                Id = fp.PostId,
                UserName = user.UserName,
                Title = fp.Post.Title,
                Date = fp.Post.Date,
                ImageUrl = fp.Post.ImageUrl,
            }).ToList();

            return Ok(favorites);
        }

        [HttpDelete("RemoveFromFavorite/{postId:int}")]
        public async Task<IActionResult> RemoveFromFavorite(int postId)
        {
            var user = await _userManager.Users
                                        .Include(u => u.Posts)
                                        .Include(e=>e.FavoritePosts)
                                        .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));
            var post = user.FavoritePosts.FirstOrDefault(e=>e.PostId == postId);

            if (post == null)
            {
                return NotFound("Post not found");
            }

            if (!user.FavoritePosts.Any(fp => fp.PostId == postId))
            {
                return BadRequest("Post already Not in favorites");
            }
            user.FavoritePosts.Remove(post);
            _db.SaveChanges();
            return NoContent();
        }
        [HttpDelete("RemovePost/id={postId}")]
        public async Task<IActionResult> RemovePost(int postId)
        {
            if (postId <= 0)
            {
                return BadRequest("Invalid Post ID.");
            }

            // Fetch the current user's ID
            var userId = _userManager.GetUserId(User);

            // Fetch the user including their posts and favorite posts relationships
            var user = await _userManager.Users
                                         .Include(u => u.Posts)
                                         .Include(u => u.FavoritePosts)
                                         .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Find the specific post to remove
            var post = user.Posts.FirstOrDefault(e => e.Id == postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            // Find and remove all related favorite records for the post
            var relatedFavorites = user.FavoritePosts.Where(fp => fp.PostId == postId).ToList();
            if (relatedFavorites.Any())
            {
                _db.FavoritePosts.RemoveRange(relatedFavorites);
            }

            // Remove the post itself
            _db.posts.Remove(post);

           
                // Save all changes to the database
                await _db.SaveChangesAsync();
                return NoContent();
        }
        [HttpGet("GetPost/id={postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            if (postId <= 0)
            {
                return BadRequest("Invalid Post ID.");
            }
            var post = _db.posts.Select(e=>new UserPostDTO
            {
                UserEmail = e.Author.Email,
                UserPhoto = e.Author.ImageUrl,
                Content = e.Content,
                Date= e.Date,
                UserName = e.Author.UserName,
                Id = e.Id,
                Title = e.Title,
                ImageUrl = e.ImageUrl
            }).SingleOrDefault(e => e.Id == postId);
            if (post == null) { return NotFound(); }
            return Ok(post);
        }
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {

            if (imageFile == null || imageFile.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_webHost.WebRootPath, "Imgs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/Imgs/" + uniqueFileName;
        }
    }
}
