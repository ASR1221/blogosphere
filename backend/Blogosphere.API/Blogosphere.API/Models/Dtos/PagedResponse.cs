namespace Blogosphere.API.Models.Dtos;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public PaginationMetadata? Metadata { get; set; }
}
