namespace Blogosphere.API.Models.Dtos;

public record FileUploadDto(
    string FileName,
    string FileUrl,
    long FileSize,
    string ContentType
);