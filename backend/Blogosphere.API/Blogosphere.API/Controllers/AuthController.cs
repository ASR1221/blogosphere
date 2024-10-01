using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blogosphere.API.Middlewares;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Blogosphere.API.Controllers
{
   [Route("api/auth")]
   [ApiController]
   public class AuthController : ControllerBase
   {
      private readonly UserManager<User> _userManager;
      private readonly SignInManager<User> _signInManager;
      private readonly IConfiguration _configuration;

      public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
      {
         _userManager = userManager;
         _signInManager = signInManager;
         _configuration = configuration;
      }

      [HttpPost("register")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<IActionResult> Register([FromBody] RegisterDto model)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

            var user = new User { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
               var token = GenerateJwtToken(user);
               return Ok(new { Token = token, Message = "User registered successfully" });
            }

            return BadRequest(result.Errors);

         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      [HttpPost("login")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<IActionResult> Login([FromBody] LoginDto model)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
               User? user = await _userManager.FindByEmailAsync(model.Email);
               if (user == null) return NotFound();

               var token = GenerateJwtToken(user);

               return Ok(new { Token = token, Message = "User logged in successfully" });
            }

            return Unauthorized();

         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      [HttpGet("refresh")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<IActionResult> Refresh()
      {
         try
         {
            if (HttpContext.Items.TryGetValue("UserId", out var userId))
            {
               User? user = await _userManager.FindByIdAsync(userId?.ToString() ?? "");
               if (user == null) return NotFound();

               var token = GenerateJwtToken(user);

               return Ok(new { Token = token, Message = "Toekn Refreshed" });
            }

            return Unauthorized();

         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      private string GenerateJwtToken(User user)
      {
         var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"] ?? ""));
         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
         var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

         var token = new JwtSecurityToken(
            _configuration["JwtIssuer"],
            _configuration["JwtAudience"],
            claims,
            expires: expires,
            signingCredentials: creds
         );

         return new JwtSecurityTokenHandler().WriteToken(token);
      }
   }
}
