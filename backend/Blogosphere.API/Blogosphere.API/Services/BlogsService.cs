using Blogosphere.API.Models;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Services;

// create blogs service based on the BlogsController

public interface IBlogsService
{
   Task<Blog> CreateBlog(AddBlogDto model, string userId);
   Task<Blog?> UpdateBlog(EditBlogDto model, int id, string userId);
   Task<bool> DeleteBlog(int blogId, string userId);
   Task<SingleBlogResponseDto?> GetBlog(int blogId, HttpContext httpContex);
   Task<PagedResponse<BlogInListResponseDto>> GetBlogs(string? category, int? page);
}

public class BlogsService : IBlogsService
{
   private readonly AppDbContext _dbContext;

   public BlogsService(AppDbContext context)
   {
      _dbContext = context;
   }

   public async Task<Blog> CreateBlog(AddBlogDto model, string userId)
   {
      var blog = new Blog
      {
         UserId = userId,
         Title = model.Title,
         Body = model.Body,
         Category = model.Category,
         Thumbnail = model.ThumbnailUrl,
         CreatedAt = DateTime.Now,
         EditedAt = DateTime.Now
      };

      _dbContext.Blogs.Add(blog);
      await _dbContext.SaveChangesAsync();

      return blog;
   }

   public async Task<Blog?> UpdateBlog(EditBlogDto model, int id, string userId)
   {
      var blog = await _dbContext.Blogs.FindAsync(id);
      if (blog == null) return null;

      if (blog.UserId == userId)
      {
         throw new UnauthorizedAccessException("You are not authorized to edit this blog");
      }
      if (!string.IsNullOrEmpty(model.Title))
      {
         blog.Title = model.Title;
      }
      if (!string.IsNullOrEmpty(model.Body))
      {
         blog.Body = model.Body;
      }
      if (!string.IsNullOrEmpty(model.Category))
      {
         blog.Category = model.Category;
      }
      if (!string.IsNullOrEmpty(model.ThumbnailUrl))
      {
         blog.Thumbnail = model.ThumbnailUrl;
      }
      blog.EditedAt = DateTime.Now;

      await _dbContext.SaveChangesAsync();

      return blog;
   }

   public async Task<bool> DeleteBlog(int blogId, string userId)
   {
      var blog = await _dbContext.Blogs.FindAsync(blogId);
      if (blog == null) return false;

      if (blog.UserId == userId)
      {
         throw new UnauthorizedAccessException("You are not authorized to delete this blog");
      }

      _dbContext.Blogs.Remove(blog);
      await _dbContext.SaveChangesAsync();

      return true;
   }

   public async Task<SingleBlogResponseDto?> GetBlog(int blogId, HttpContext httpContext)
   {
      var blog = await _dbContext.Blogs.FindAsync(blogId);
      if (blog == null) return null;

      var like = await _dbContext.Likes
         .FirstOrDefaultAsync(l => l.BlogId == blogId && l.UserId == (string)httpContext.Items["UserId"]);

      SingleBlogResponseDto responseBlog = new(
            Id: blog.Id,
            AutherId: blog.UserId,
            AutherImage: blog?.User?.Image ?? "",
            AutherName: blog?.User?.UserName ?? "",
            Title: blog?.Title ?? "",
            ThumbnailUrl: blog?.Thumbnail ?? "",
            Category: blog?.Category ?? "",
            Body: blog?.Body ?? "",
            CreatedAt: blog?.CreatedAt ?? DateTime.Now,
            LikesCount: blog?.LikesCount ?? 0,
            CommentsCount: blog?.CommentsCount ?? 0,
            IsLikedByUser: like != null
         );


      return responseBlog;
   }

   public async Task<PagedResponse<BlogInListResponseDto>> GetBlogs(string? category, int? page)
   {
      if (category == null || category == "")
      {
         List<Blog> b = await _dbContext.Blogs
            .OrderByDescending(b => b.CreatedAt)
            .Take(6)
            .ToListAsync();

         PaginationMetadata m = new()
         {
            CurrentPage = 1,
            PageSize = 6,
            TotalCount = 6,
            TotalPages = 1
         };

         List<BlogInListResponseDto> res = [];

         if (b == null) throw new Exception("An error occurred");
         foreach (Blog blog in b)
         {
            res.Add(new(
               Id: blog.Id,
               AutherId: blog.UserId,
               AutherImage: blog?.User?.Image ?? "",
               AutherName: blog?.User?.UserName ?? "",
               Title: blog?.Title ?? "",
               ThumbnailUrl: blog?.Thumbnail ?? "",
               CreatedAt: blog?.CreatedAt ?? DateTime.Now,
               LikesCount: blog?.LikesCount ?? 0,
               CommentsCount: blog?.CommentsCount ?? 0
            ));
         }

         return new PagedResponse<BlogInListResponseDto>
         {
            Data = res,
         };
      }

      page ??= 16;
      int PageSize = 16;
      var query = _dbContext.Blogs.AsNoTracking();

      var totalCount = await query.CountAsync();
      var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

      var skipValue = (int)((page - 1) * PageSize);
      var blogs = await query
         .OrderByDescending(b => b.CreatedAt)
         .Skip(skipValue)
         .Take(PageSize)
         .ToListAsync();

      List<BlogInListResponseDto> responseBlogs = [];

      if (blogs == null) throw new Exception("An error occurred");
      foreach (Blog blog in blogs)
      {
         responseBlogs.Add(new(
            Id: blog.Id,
            AutherId: blog.UserId,
            AutherImage: blog?.User?.Image ?? "",
            AutherName: blog?.User?.UserName ?? "",
            Title: blog?.Title ?? "",
            ThumbnailUrl: blog?.Thumbnail ?? "",
            CreatedAt: blog?.CreatedAt ?? DateTime.Now,
            LikesCount: blog?.LikesCount ?? 0,
            CommentsCount: blog?.CommentsCount ?? 0
         ));
      }

      var metadata = new PaginationMetadata
      {
         CurrentPage = page ?? 1,
         PageSize = PageSize,
         TotalCount = totalCount,
         TotalPages = totalPages
      };

      return new PagedResponse<BlogInListResponseDto>
      {
         Data = responseBlogs,
         Metadata = metadata
      };
   }

}
