using Blogosphere.API.Models;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;

namespace Blogosphere.API.Services;

public interface IUserService
{
   Task<User?> Edit(string userId, EditUserDto editUserDto);
   Task<bool> Delete(string userId);
}

public class UserService : IUserService
{
   private readonly AppDbContext _dbContext;

   public UserService(AppDbContext dbContext) => _dbContext = dbContext;

   public async Task<User?> Edit(string userId, EditUserDto editUserDto)
   {
      var user = await _dbContext.Users.FindAsync(userId);
      if (user == null) return null;

      if (user.Id == userId)
      {
         throw new UnauthorizedAccessException("You are not authorized to edit this blog");
      }
      if (string.IsNullOrEmpty(editUserDto.Name) && string.IsNullOrEmpty(editUserDto.Image)) return user;
      if (!string.IsNullOrEmpty(editUserDto.Name)) user.UserName = editUserDto.Name;
      if (!string.IsNullOrEmpty(editUserDto.Image)) user.Image = editUserDto.Image;

      user.EditedAt = DateTime.Now;
      
      await _dbContext.SaveChangesAsync();

      return user;
   }

   public async Task<bool> Delete(string userId)
   {
      var user = await _dbContext.Users.FindAsync(userId);
      if (user == null) return false;

      if (user.Id == userId)
      {
         throw new UnauthorizedAccessException("You are not authorized to edit this blog");
      }

      _dbContext.Users.Remove(user);
      await _dbContext.SaveChangesAsync();

      return true;
   }
   
}
