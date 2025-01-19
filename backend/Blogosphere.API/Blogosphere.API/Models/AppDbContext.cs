using Blogosphere.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Models
{
   public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
   {
      protected override void OnModelCreating(ModelBuilder builder)
      {
         base.OnModelCreating(builder);

         // Change AspNetUsers to users
         builder.Entity<User>(entity =>
         {
               entity.ToTable("users");
         });

         // Customize other Identity table names if needed
         builder.Entity<IdentityRole>(entity =>
         {
               entity.ToTable("roles");
         });

         builder.Entity<IdentityUserRole<string>>(entity =>
         {
               entity.ToTable("user_roles");
         });

         builder.Entity<IdentityUserClaim<string>>(entity =>
         {
               entity.ToTable("user_claims");
         });

         builder.Entity<IdentityUserLogin<string>>(entity =>
         {
               entity.ToTable("user_logins");
         });

         builder.Entity<IdentityUserToken<string>>(entity =>
         {
               entity.ToTable("user_tokens");
         });

         builder.Entity<IdentityRoleClaim<string>>(entity =>
         {
               entity.ToTable("role_claims");
         });
      }

      public DbSet<User> Users { get; set; }
      public DbSet<Blog> Blogs { get; set; }
      public DbSet<Like> Likes { get; set; }
      public DbSet<Comment> Comments { get; set; }
   }
}