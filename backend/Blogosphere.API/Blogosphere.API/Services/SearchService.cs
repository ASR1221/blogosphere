using Blogosphere.API.Models;
using Blogosphere.API.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Services;

public interface ISearchService
{
   Task<SearchResultDto> SmartSearch(
     string searchTerm,
     string category,
     int page,
     int pageSize
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
   )
   {
      if (string.IsNullOrWhiteSpace(searchTerm))
         return new SearchResultDto(
            Blogs: new PagedResponse<BlogDto>
            {
               Data = [],
               Metadata = new PaginationMetadata
               {
                  CurrentPage = 1,
                  PageSize = 16,
                  TotalCount = 0,
                  TotalPages = 1
               }
            },
            Authers: []
         );

      searchTerm = searchTerm.ToLower();

      // First get all matching blogs from database
      var blogsQuery = _context.Blogs
         .AsNoTracking()
         .Where(b => b.Title.ToLower().Contains(searchTerm));

      if (!string.IsNullOrEmpty(category))
      {
         blogsQuery = blogsQuery.Where(b => (b.Category ?? "").ToLower().Contains(category));
      }

      // Get all matching blogs
      var allMatchingBlogs = await blogsQuery
         .Select(b => new
         {
            b.Id,
            b.Title,
            b.Body,
            b.Category,
            b.CommentsCount,
            b.LikesCount,
            b.Thumbnail,
            b.CreatedAt,
            b.User
         })
         .Take(1000) // to prevent out of memory
         .ToListAsync();

      // Apply Levenshtein Distance in memory
      var tempFilteredBlogs = allMatchingBlogs
         .Select(b => new
         {
            Blog = b,
            TitleRelevance = ComputeLevenshteinDistance(b.Title.ToLower(), searchTerm)
         })
         .Where(b => b.TitleRelevance > 0.4);

      var filteredBlogs = tempFilteredBlogs
         .OrderByDescending(b => b.TitleRelevance)
         .Skip((page - 1) * pageSize)
         .Take(pageSize)
         .ToList();

      if (filteredBlogs == null) throw new Exception("An error occurred");

      var blogsResponse = filteredBlogs.Select(blog => new BlogDto(
         Id: blog.Blog.Id,
         AutherId: blog.Blog.User?.Id ?? "",
         AutherImage: blog.Blog.User?.Image ?? "",
         AutherName: blog.Blog.User?.UserName ?? "",
         Title: blog.Blog.Title ?? "",
         ThumbnailUrl: blog.Blog.Thumbnail ?? "",
         CreatedAt: blog.Blog.CreatedAt,
         LikesCount: blog.Blog.LikesCount,
         CommentsCount: blog.Blog.CommentsCount
      )).ToList();

      var totalCount = tempFilteredBlogs.ToList().Count;
      var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

      List<UserSearchResult> usersResponse = [];

      if (string.IsNullOrEmpty(category))
      {
         // Get matching users from database
         var matchingUsers = await _context.Users
            .AsNoTracking()
            .Where(b => (b.UserName ?? "").ToLower().Contains(searchTerm))
            .Select(a => new
            {
               a.Id,
               a.UserName,
               a.Image
            })
            .ToListAsync();

         // Apply Levenshtein Distance in memory
         usersResponse = matchingUsers
            .Select(u => new
            {
               User = u,
               NameRelevance = ComputeLevenshteinDistance((u.UserName ?? "").ToLower(), searchTerm)
            })
            .Where(u => u.NameRelevance > 0.5)
            .OrderByDescending(u => u.NameRelevance)
            .Take(16)
            .Select(u => new UserSearchResult(
               Id: u.User.Id,
               Name: u.User.UserName ?? "",
               Image: u.User.Image ?? ""
            ))
            .ToList();
      }

      var result = new SearchResultDto
      (
         Blogs: new PagedResponse<BlogDto>
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