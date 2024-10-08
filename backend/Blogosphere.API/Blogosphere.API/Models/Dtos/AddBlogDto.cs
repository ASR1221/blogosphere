using System.ComponentModel.DataAnnotations;

namespace Blogosphere.API.Models.Dtos;

public record class AddBlogDto
{
   [Required]
   public required string Title { get; set; }

   [Required]
   public required string ThumbnailUrl { get; set; }

   [Required]
   public required string Category { get; set; }

   [Required]
   public required string Body { get; set; }
}
