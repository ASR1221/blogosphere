using System.ComponentModel.DataAnnotations.Schema;

namespace Blogosphere.API.Models.Entities
{
    public class Blog : BaseModel
    {
        [ForeignKey("User")]
        public required string UserId { get; set; }

        public User? User { get; set; }

        public string? Category { get; set; }

        public required string Title { get; set; }

        public required string Body { get; set; }

        public required string Thumbnail { get; set; }

        public int LikesCount { get; set; } = 0;

        public int CommentsCount { get; set; } = 0;

    }
}