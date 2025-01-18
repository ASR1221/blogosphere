using Blogosphere.API.Models.Entities;

namespace Blogosphere.API.Models.Dtos;

public record SearchResultDto(
    PagedResponse<BlogInListResponseDto> Blogs,
    List<UserSearchResult> Authers
);

public record UserSearchResult(
    string Id,
    string Name,
    string Image
);