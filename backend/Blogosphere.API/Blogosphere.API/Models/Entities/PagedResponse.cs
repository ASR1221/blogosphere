namespace Blogosphere.API.Models.Entities;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public PaginationMetadata? Metadata { get; set; }
}
