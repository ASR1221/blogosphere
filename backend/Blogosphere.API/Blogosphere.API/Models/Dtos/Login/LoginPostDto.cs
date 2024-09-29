using System.ComponentModel.DataAnnotations;

namespace Blogosphere.API.Models.Dtos.Login;

public record class LoginPostDto
{
   [EmailAddress(ErrorMessage = "Invalid Email Address")]
   public required string Email { get; set; }
   
   public required string Password { get; set; }
}
