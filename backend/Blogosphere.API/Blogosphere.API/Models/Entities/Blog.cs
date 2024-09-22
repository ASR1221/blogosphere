using System.ComponentModel.DataAnnotations.Schema;

namespace Blogosphere.API.Models.Entities
{
    public class Blog : BaseModel
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        public User? User { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public required string Title { get; set; }

        public required string Body { get; set; }

        public required string Thumbnail { get; set; }

        public int LikesCount { get; set; } = 0;

        public int CommentsCount { get; set; } = 0;

    }
}