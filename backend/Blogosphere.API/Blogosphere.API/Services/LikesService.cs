using Blogosphere.API.Models;
using Blogosphere.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Services;

public interface ILikesService
{
   Task<bool> Add(int blogId, string userId);
   Task<bool> Delete(int likeId, string userId);
}

public class LikesService : ILikesService
{

   private readonly AppDbContext _dbContext;

   public LikesService(AppDbContext dbContext) => _dbContext = dbContext;

   public async Task<bool> Add(int blogId, string userId) {
      var dbLike = await _dbContext.Likes.Where(l => l.BlogId == blogId && l.UserId == userId).FirstOrDefaultAsync();

      if (dbLike != null) return true;

      var blog = await _dbContext.Blogs.FindAsync(blogId);
      if (blog == null) return false;

      var like = new Like
      {
         BlogId = blogId,
         UserId = userId
      };

      blog.LikesCount++;
      _dbContext.Likes.Add(like);
      await _dbContext.SaveChangesAsync();

      return true;
   }

   public async Task<bool> Delete(int likeId, string userId) {
      var like = await _dbContext.Likes.FindAsync(likeId);
      if (like == null) return false;

      if (like.UserId != userId)
      {
         throw new UnauthorizedAccessException("You are not authorized to delete this like");
      }
      
      _dbContext.Blogs.Where(b => b.Id == like.BlogId).First().LikesCount--; 
      _dbContext.Likes.Remove(like);  
      await _dbContext.SaveChangesAsync();

      return true;
   }

}
