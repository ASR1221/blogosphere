using Blogosphere.API.Models;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Services;

public interface ICommentsService
{
   Task<Comment?> Create(int blogId, string userId, AddCommentDto body);
   Task<Comment?> Update(int commentId, string userId, AddCommentDto body);
   Task<bool> Delete(int blogId, string userId);
   Task<PagedResponse<Comment>> Get(int blogId, int? page);
}

public class CommentsService : ICommentsService
{

   private readonly AppDbContext _dbContext;

   public CommentsService(AppDbContext context) => _dbContext = context;

   public async Task<Comment?> Create(int blogId, string userId, AddCommentDto model)
   {

      var blog = await _dbContext.Blogs.FindAsync(blogId);
      if (blog == null) return null;

      var comment = new Comment
      {
         UserId = userId,
         Body = model.Body,
         BlogId = blogId,
         CreatedAt = DateTime.Now,
         EditedAt = DateTime.Now
      };

      _dbContext.Comments.Add(comment);
      await _dbContext.SaveChangesAsync();

      return comment;
   }
   
   public async Task<Comment?> Update(int commentId, string userId, AddCommentDto body)
   {
      var comment = await _dbContext.Comments.FindAsync(commentId);
      if (comment == null) return null;

      if (comment.UserId == userId)
      {
         throw new UnauthorizedAccessException("You are not authorized to edit this blog");
      }
      if (!string.IsNullOrEmpty(body.Body))
      {
         comment.Body = body.Body;
      }
      comment.EditedAt = DateTime.Now;

      await _dbContext.SaveChangesAsync();

      return comment;
   }

   public async Task<bool> Delete(int commentId, string userId)
   {
      var comment = await _dbContext.Blogs.FindAsync(commentId);
      if (comment == null) return false;

      if (comment.UserId == userId)
      {
         throw new UnauthorizedAccessException("You are not authorized to delete this blog");
      }

      _dbContext.Blogs.Remove(comment);
      await _dbContext.SaveChangesAsync();

      return true;
   }

   public async Task<PagedResponse<Comment>> Get(int blogId, int? page)
   {
      page ??= 1;
      int PageSize = 16;
      var query = _dbContext.Comments.AsNoTracking().Where(c => c.BlogId == blogId);;

      var totalCount = await query.CountAsync();
      var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

      var skipValue = (int)((page - 1) * PageSize);
      var comments = await query
         .OrderByDescending(b => b.CreatedAt)
         .Skip(skipValue)
         .Take(PageSize)
         .ToListAsync();

      if (comments == null) throw new Exception("An error occurred");

      var metadata = new PaginationMetadata
      {
         CurrentPage = page ?? 1,
         PageSize = PageSize,
         TotalCount = totalCount,
         TotalPages = totalPages
      };

      return new PagedResponse<Comment>
      {
         Data = comments,
         Metadata = metadata
      };
   }
}
