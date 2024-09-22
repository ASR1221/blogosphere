using Blogosphere.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Models
{
   public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
   {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
   }
}