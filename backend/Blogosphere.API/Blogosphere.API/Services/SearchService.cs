using Blogosphere.API.Models;
using Blogosphere.API.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Services;

public interface ISearchService
{
    Task<SearchResultDto> SmartSearch(
      string searchTerm, 
      string category = "", 
      int page = 1, 
      int pageSize = 16
   );
}

public class SearchService : ISearchService
{
   private readonly AppDbContext _context;

   public SearchService(AppDbContext context)
   {
      _context = context;
   }

   public async Task<SearchResultDto> SmartSearch(
      string searchTerm, 
      string category = "",
      int page = 1, 
      int pageSize = 16
   ) {
      if (string.IsNullOrWhiteSpace(searchTerm))
         return new SearchResultDto(
            Blogs: new PagedResponse<BlogInListResponseDto>{
               Data = [],
               Metadata = new PaginationMetadata{
                  CurrentPage = 1,
                  PageSize = 16,
                  TotalCount = 0,
                  TotalPages = 1
               }
            },
            Authers: []
         );

      searchTerm = searchTerm.ToLower();

      var skipValue = (page - 1) * pageSize;

      var blogsQuery = _context.Blogs
        .AsNoTracking()
        .Where(b => b.Title.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));

      if (!string.IsNullOrEmpty(category))
      {
         blogsQuery = blogsQuery.Where(b => (b.Category ?? "").Equals(category, StringComparison.CurrentCultureIgnoreCase));
      }
      // Search blogs
      var blogs = await blogsQuery.Select(b => new
         {
            b.Id,
            b.Title,
            b.Body,
            b.Category,
            b.CommentsCount,
            b.LikesCount,
            b.Thumbnail,
            b.CreatedAt,
            b.User,
            TitleRelevance = ComputeLevenshteinDistance(b.Title.ToLower(), searchTerm),
         })
         .Where(b => b.TitleRelevance > 0.4)
         .OrderByDescending(b => b.TitleRelevance)
         .Skip(skipValue)
         .Take(pageSize)
         .ToListAsync();

      if (blogs == null) throw new Exception("An error occurred");

      List<BlogInListResponseDto> blogsResponse = [];

      var totalCount = blogs.Count;
      var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

      foreach (var blog in blogs)
      {
         blogsResponse.Add(new(
            Id: blog.Id,
            AutherId: blog?.User?.Id ?? "",
            AutherImage: blog?.User?.Image ?? "",
            AutherName: blog?.User?.UserName ?? "",
            Title: blog?.Title ?? "",
            ThumbnailUrl: blog?.Thumbnail ?? "",
            CreatedAt: blog?.CreatedAt ?? DateTime.Now,
            LikesCount: blog?.LikesCount ?? 0,
            CommentsCount: blog?.CommentsCount ?? 0
         ));
      }


      List<UserSearchResult> usersResponse = [];

      if (string.IsNullOrEmpty(category))
      {
         var users = await _context.Users
               .AsNoTracking()
               .Where(b => (b.UserName ?? "").Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
               .Select(a => new
               {
                  a.Id,
                  a.UserName,
                  a.Image,
                  NameRelevance = ComputeLevenshteinDistance((a.UserName ?? "").ToLower() ?? "", searchTerm)
               })
               .Where(a => a.NameRelevance > 0.5)
               .OrderByDescending(b => b.NameRelevance)
               .Take(16)
               .ToListAsync();

         if (users == null) throw new Exception("An error occurred");
         foreach (var user in users)
         {
               usersResponse.Add(new(
                  Id: user.Id,
                  Name: user.UserName ?? "",
                  Image: user.Image ?? ""
               ));
         }
      }

      var result = new SearchResultDto
      (
         Blogs: new PagedResponse<BlogInListResponseDto>
         {
            Data = blogsResponse,
            Metadata = new PaginationMetadata
            {
               CurrentPage = page,
               PageSize = pageSize,
               TotalCount = totalCount,
               TotalPages = totalPages
            }
         },
         Authers: usersResponse
      );

      return result;
   }

   private double ComputeLevenshteinDistance(string source, string target)
   {
      if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0;

      // Check if source contains target (partial match)
      if (source.Contains(target))
         return 1.0;

      var sourceLength = source.Length;
      var targetLength = target.Length;
      var matrix = new int[sourceLength + 1, targetLength + 1];

      // Initialize first row and column
      for (var i = 0; i <= sourceLength; i++)
         matrix[i, 0] = i;
      for (var j = 0; j <= targetLength; j++)
         matrix[0, j] = j;

      // Fill matrix
      for (var i = 1; i <= sourceLength; i++)
      {
         for (var j = 1; j <= targetLength; j++)
         {
            var cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + cost
            );
         }
      }

      // Calculate similarity score (0 to 1)
      var maxLength = Math.Max(sourceLength, targetLength);
      var distance = matrix[sourceLength, targetLength];
      return 1 - ((double)distance / maxLength);
   }
}