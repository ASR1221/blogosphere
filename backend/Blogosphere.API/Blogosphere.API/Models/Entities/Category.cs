using System.ComponentModel.DataAnnotations;

namespace Blogosphere.API.Models.Entities;

public class Category : BaseModel
{
   public required string Title { get; set; }
}
