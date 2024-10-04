using Blogosphere.API.Middlewares;
using Blogosphere.API.Models;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Controllers
{
   [ApiController]
   [Route("api/blogs")]
   public class BlogsController : ControllerBase
   {

      private readonly AppDbContext _dbContext;

      public BlogsController(AppDbContext context)
      {
         _dbContext = context;
      }

      [HttpGet]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<PagedResponse<BlogResponseDto>>> Get(
         [FromQuery(Name = "category")] string? category,
         [FromQuery(Name = "page")] int? page
      )
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

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

               return Ok(new PagedResponse<Blog>
               {
                  Data = b,
               });
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

            List<BlogResponseDto> responseBlogs = [];

            if (blogs == null) return Problem(statusCode: StatusCodes.Status500InternalServerError);
            foreach (Blog blog in blogs)
            {
               responseBlogs.Add(new(
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

            return Ok(new PagedResponse<BlogResponseDto>
            {
               Data = responseBlogs,
               Metadata = metadata
            });
         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      [HttpGet("{id}")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<BlogResponseDto>> Get([FromRoute] int id)
      {
         try
         {
            var blog = await _dbContext.Blogs
               .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
               return NotFound();
            }

            BlogResponseDto responseBlog = new(
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
                  CommentsCount: blog?.CommentsCount ?? 0
               );

            return Ok(blog);
         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      [HttpPost]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<SuccessResponseDto>> Post([FromBody] AddBlogDto body)
      {
         try
         {

            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

            Blog newBlog = new()
            {
               UserId = (string)HttpContext.Items["UserId"],
               Title = body.Title,
               Thumbnail = body.ThumbnailUrl,
               Category = body.Categoriy,
               Body = body.Body
            };

            _dbContext.Blogs.Add(newBlog);
            await _dbContext.SaveChangesAsync();

            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Blog Added Successfully"
            ));

         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      [HttpPatch("{id}")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<SuccessResponseDto>> Patch([FromRoute] int id, [FromBody] EditBlogDto body)
      {
         try
         {

            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

            Blog? blog = await _dbContext.Blogs.FindAsync(id);

            if (blog == null)
            {
               return NotFound();
            }

            if (blog.Id == (int)HttpContext.Items["UserId"])
            {
               return Unauthorized();
            }

            if (body.Title != null)
            {
               blog.Title = body.Title;
            }

            if (body.ThumbnailUrl != null)
            {
               blog.Thumbnail = body.ThumbnailUrl;
            }

            if (body.Body != null)
            {
               blog.Body = body.Body;
            }

            if (body.Category != null)
            {
               blog.Category = body.Category;
            }

            blog.EditedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Blog Edited Successfully"
            ));

         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      [HttpDelete("{id}")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<SuccessResponseDto>> Delete([FromRoute] int id)
      {
         try
         {

            if (!ModelState.IsValid)
            {
               return BadRequest();
            }

            var blog = await _dbContext.Blogs.FindAsync(id);
            if (blog == null)
            {
               return NotFound();
            }

            if (blog.Id == (int)HttpContext.Items["UserId"])
            {
               return Unauthorized();
            }

            _dbContext.Blogs.Remove(blog);
            await _dbContext.SaveChangesAsync();

            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Blog Deleted Successfully"
            ));

         }
         catch (Exception ex)
         {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }
   }
}
