using Microsoft.AspNetCore.Identity;

namespace Blogosphere.API.Models.Entities;

public class User : IdentityUser
{
   public string? Image { get; set; }

   public DateTime CreatedAt { get; set; } = DateTime.Now;

   public DateTime EditedAt { get; set; } = DateTime.Now;

}