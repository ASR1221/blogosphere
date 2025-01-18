using Blogosphere.API.Models.Entities;

namespace Blogosphere.API.Models.Dtos;

public record SearchResultDto(
    PagedResponse<BlogDto> Blogs,
    List<UserSearchResult> Authers
);

public record UserSearchResult(
    string Id,
    string Name,
    string Image
);