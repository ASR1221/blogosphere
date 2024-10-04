using System.ComponentModel.DataAnnotations;

namespace Blogosphere.API.Models.Dtos;

public record class EditBlogDto
{
   public string? Title { get; set; }

   public string? ThumbnailUrl { get; set; }

   public string? Category { get; set; }

   public string? Body { get; set; }
}
