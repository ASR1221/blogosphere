using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Models.Entities;

[Index(nameof(Email), IsUnique = true)]
public class User : BaseModel
{
   public required string Name { get; set; }

   public required string Email { get; set; }

   public required string Password { get; set; }

   public string? Image { get; set; }

   public bool IsVerified { get; set; } = false;
}
