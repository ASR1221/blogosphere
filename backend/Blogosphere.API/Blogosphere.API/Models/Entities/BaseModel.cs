using System.ComponentModel.DataAnnotations;

namespace Blogosphere.API.Models.Entities;

public class BaseModel
{
   [Key]
   public int Id { get; set; }

   public DateTime CreatedAt { get; set; } = DateTime.Now;

   public DateTime EditedAt { get; set; } = DateTime.Now;
}
