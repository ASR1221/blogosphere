namespace Blogosphere.API.Models.Dtos;

public record BlogDto(
   int Id,
   string AutherId,
   string AutherImage,
   string AutherName,
   string Title,
   string ThumbnailUrl,
   DateTime CreatedAt,
   int LikesCount,
   int CommentsCount
);