namespace Blogosphere.API.Models.Dtos;

public record SingleBlogResponseDto(
   int Id,
   string AutherId,
   string AutherImage,
   string AutherName,
   string Title,
   string ThumbnailUrl,
   string Category,
   string Body,
   DateTime CreatedAt,
   int LikesCount,
   int CommentsCount,
   bool IsLikedByUser
);
