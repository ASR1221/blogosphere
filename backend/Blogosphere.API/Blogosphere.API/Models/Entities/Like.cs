using System.ComponentModel.DataAnnotations.Schema;

namespace Blogosphere.API.Models.Entities;

public class Like : BaseModel
{
   [ForeignKey("User")]
   public int UserId { get; set; }

   public User? User { get; set; }

   [ForeignKey("Blog")]
   public int BlogId { get; set; }
}
