using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Nasa.Models;
using Nasa.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace Nasa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        IConfiguration _configuration;
        UserManager<ApplicationUser> _userManager;
        RoleManager<IdentityRole> _roleManager;
        IWebHostEnvironment _webHost;
        SignInManager<ApplicationUser> _signInManager;
        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration,RoleManager<IdentityRole> roleManager
            , SignInManager<ApplicationUser> signInManager , IWebHostEnvironment webHost)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _webHost = webHost;
        }
        //create new user "Regestration" Post
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registration([FromForm]RegUserDTO RD , [FromForm] IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (_roleManager.Roles.Count()==0)
                {

                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                string ImagePath = null;
                if (image != null) {
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

                    ImagePath = await SaveImageAsync(image);
                }
                var user = new ApplicationUser
                {
                    UserName = RD.UserName,
                    Email = RD.Email,
                    ImageUrl = ImagePath
                };
                if (await _userManager.FindByNameAsync(user.UserName)!=null )
                {
                    ModelState.AddModelError("userName","User Name Should Be Unique");
                    return BadRequest(ModelState);
                }

                IdentityResult res = await _userManager.CreateAsync(user, RD.PassWord);
                if (res.Succeeded)
                {
                    if (_userManager.Users.Count() == 1)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                    return Ok("added with success");
                }
                else
                {
                    return BadRequest(res.Errors.FirstOrDefault());
                }
            }

            return BadRequest(ModelState);


        }
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LogDTO LogUSer)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(LogUSer.UserName);
                if (user != null)
                {
                    var ch = await _userManager.CheckPasswordAsync(user, LogUSer.Password);
                    if (ch)
                    {
                        // claims token
                        var claim = new List<Claim>();
                        claim.Add(new Claim(ClaimTypes.Name, user.UserName));
                        claim.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        claim.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        var roles = await _userManager.GetRolesAsync(user);
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                        foreach (var item in roles)
                        {
                            claim.Add(new Claim(ClaimTypes.Role, item));
                        }
                        JwtSecurityToken token = new JwtSecurityToken(
                                issuer: _configuration["JWT:ISS"],
                                audience: _configuration["JWT:aud"],
                                claims: claim,
                                expires: DateTime.Now.AddHours(1),
                                signingCredentials: signingCredentials
                            );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            exp = token.ValidTo,
              
                        });
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
            return Unauthorized();
        }
        // "Login" Post
       
        [HttpPost("ForgetPassword/userName={userName}&newPassword={newPassword}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForgetPassword(string userName , string newPassword)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null) { 
            var token =await _userManager.GeneratePasswordResetTokenAsync(user);
                var res = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (res.Succeeded) {
                    return Ok("Password Changed Succesfly");
                }
                else
                {
                    return BadRequest(res.Errors.First());  
                }
            }
            else
            {
                return NotFound("User Name Not Found");
            }
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
