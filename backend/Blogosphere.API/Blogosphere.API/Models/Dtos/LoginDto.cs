using System.ComponentModel.DataAnnotations;

namespace Blogosphere.API.Models.Dtos;

public record class LoginDto
{
   [Required]
   [EmailAddress]
   public required string Email { get; set; }

   [Required]
   [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
   [DataType(DataType.Password)]
   public required string Password { get; set; }
}