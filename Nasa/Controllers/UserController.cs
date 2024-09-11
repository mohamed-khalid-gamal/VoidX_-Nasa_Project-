using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nasa.DTO;
using Nasa.Models;
using System.Linq;

namespace Nasa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserManager<ApplicationUser>_userManager,SignInManager<ApplicationUser> _signInManager , RoleManager<IdentityRole> _roleManager ,IWebHostEnvironment _webHost) : ControllerBase
    {
        [HttpGet("IsAdmin")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> IsAdmin()
        {

            if (User.IsInRole("Admin"))
            {
                return Ok("True");
            }
            else
            {
                return Ok("False");
            }


        }
        [HttpGet("GetUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser()
        {

            var x = await _userManager.GetUserAsync(User);
            var user = new GetUserDTO()
            {
                Phone = x.PhoneNumber,
                UserName = x.UserName,
                Email = x.Email,
                ImageUrl = x.ImageUrl
            };
            return Ok(user);
        }
        [HttpPut("UpdateUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser([FromForm] UserDTO user , [FromForm] IFormFile image )
        {
            if (ModelState.IsValid)
            {
                var OldUser = await _userManager.GetUserAsync(User);
                OldUser.PhoneNumber = user.Phone;
                OldUser.UserName = user.UserName;
                OldUser.Email = user.Email;
                if (image != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
                    }

                    // Validate MIME type for additional security (optional)
                    var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
                    if (!allowedMimeTypes.Contains(image.ContentType.ToLower()))
                    {
                        return BadRequest("Invalid image file type.");
                    }


                    OldUser.ImageUrl =await SaveImageAsync(image);
                }
                await _userManager.UpdateAsync(OldUser);
                return NoContent();
            }
            else
            {
                return BadRequest(ModelState);
            }

        }
        [HttpPut("UpdateUserRole/role={role}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateUserRole(string role)
        {
            if (await _roleManager.FindByNameAsync(role) != null)
            {
                await _userManager.AddToRoleAsync(await _userManager.GetUserAsync(User), role);
            }
            else
            {
                return BadRequest("There is No Role with that name");
            }

            return NoContent();
        }
        [HttpPut("UpdateUserPassword/newPassword={newPassword}&currentPassword={currentPassword}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateUserPassword(string newPassword, string currentPassword)
        {
            var res = await _userManager.ChangePasswordAsync(await _userManager.GetUserAsync(User), currentPassword, newPassword);
            if (res.Succeeded)
            {

                return NoContent();
            }
            return BadRequest(res.Errors.First());
        }
        [Authorize(Roles ="Admin")]
        [HttpGet("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.Select(e => new GetUserDTO
            {
                UserName = e.UserName,
                Phone = e.PhoneNumber,
                Email = e.Email,
                ImageUrl = e.ImageUrl
            }).ToListAsync();
            return Ok(users);
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
