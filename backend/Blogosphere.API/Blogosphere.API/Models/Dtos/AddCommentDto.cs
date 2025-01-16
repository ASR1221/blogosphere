using System.ComponentModel.DataAnnotations;

namespace Blogosphere.API.Models.Dtos;

public record class AddCommentDto
{
   [Required]
   public required string Body { get; set; }
}
