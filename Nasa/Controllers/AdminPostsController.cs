using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class AdminPostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHost;

        public AdminPostsController(ApplicationDbContext context, IWebHostEnvironment webHost)
        {
            _context = context;
            _webHost = webHost;
        }

        // GET: api/AdminPosts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminPost>>> GetadminPosts()
        {
            return await _context.adminPosts.ToListAsync();
        }

        // GET: api/AdminPosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminPost>> GetAdminPost(int id)
        {
            if (id<=0)
            {
                return BadRequest();
            }
            var adminPost = await _context.adminPosts.FindAsync(id);

            if (adminPost == null)
            {
                return NotFound();
            }

            return adminPost;
        }

        // PUT: api/AdminPosts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAdminPost(int id, [FromForm]AdminPostDTO adminPost )
        {
            var adpost = _context.adminPosts.FirstOrDefault(e => e.Id == id);
            if (adpost == null)
            {
                return BadRequest();
            }
            if (_context.adminPosts.Where(e=>e.Id!=id).FirstOrDefault(e=>e.Title==adminPost.Title)!=null)
            {
                return BadRequest("Title Should be uniqe");
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(adminPost.Image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
            }

            // Validate MIME type for additional security (optional)
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedMimeTypes.Contains(adminPost.Image.ContentType.ToLower()))
            {
                return BadRequest("Invalid image file type.");
            }

            var ImageUrl = await SaveImageAsync(adminPost.Image);
            adpost.ImageUrl = ImageUrl;
            adpost.Title = adminPost.Title;
            adpost.Content = adminPost.Content;
            _context.Entry(adpost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminPostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AdminPosts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminPost>> PostAdminPost([FromForm] AdminPostDTO adminPost)
        {
            if (ModelState.IsValid)
            {
                if (_context.adminPosts.FirstOrDefault(e=>e.Title==adminPost.Title)!=null)
                {
                    ModelState.AddModelError("title", "title should be uniqe");
                    return BadRequest(ModelState);
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(adminPost.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
                }

                // Validate MIME type for additional security (optional)
                var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedMimeTypes.Contains(adminPost.Image.ContentType.ToLower()))
                {
                    return BadRequest("Invalid image file type.");
                }

                var ImageUrl = await SaveImageAsync(adminPost.Image);
                var post = new AdminPost { ImageUrl = ImageUrl, Content = adminPost.Content, Id = adminPost.Id, Title = adminPost.Title };
                _context.adminPosts.Add(post);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetAdminPost", new { id = adminPost.Id }, adminPost);           
            }
            return BadRequest(ModelState);
        }

        // DELETE: api/AdminPosts/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> DeleteAdminPost(int id)
        {
            if (id<=0)
            {
                return BadRequest("invalid id ");
            }
            var adminPost = await _context.adminPosts.FindAsync(id);
            if (adminPost == null)
            {
                return NotFound();
            }

            _context.adminPosts.Remove(adminPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool AdminPostExists(int id)
        {
            return _context.adminPosts.Any(e => e.Id == id);
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
