using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public interface IAuthService
{
   Task<SuccessResponseDto> Register(RegisterDto model);
   Task<SuccessResponseDto?> Login(LoginDto model);
   Task<SuccessResponseDto?> Refresh(string userId);
}

public class AuthService : IAuthService
{
   private readonly UserManager<User> _userManager;
   private readonly SignInManager<User> _signInManager;
   private readonly IConfiguration _configuration;

   public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
   {
      _userManager = userManager;
      _signInManager = signInManager;
      _configuration = configuration;
   }

   public async Task<SuccessResponseDto> Register(RegisterDto model)
   {
      var user = new User { UserName = model.UserName, Email = model.Email };
      var result = await _userManager.CreateAsync(user, model.Password);

      if (result.Succeeded)
      {
         var token = GenerateJwtToken(user);
         return new SuccessResponseDto(token, "User registered successfully");
      }

      throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
   }

   public async Task<SuccessResponseDto?> Login(LoginDto model)
   {
      var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

      if (result.Succeeded)
      {
         User? user = await _userManager.FindByEmailAsync(model.Email);
         if (user == null) return null;

         var token = GenerateJwtToken(user);
         return new SuccessResponseDto(token, "User logged in successfully");
      }

      throw new UnauthorizedAccessException("Invalid login attempt.");
   }

   public async Task<SuccessResponseDto?> Refresh(string userId)
   {
      User? user = await _userManager.FindByIdAsync(userId);
      if (user == null) return null;

      var token = GenerateJwtToken(user);
      return new SuccessResponseDto(token, "Token refreshed");
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
