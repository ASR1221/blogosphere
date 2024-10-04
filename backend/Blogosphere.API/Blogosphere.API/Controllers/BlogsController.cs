using Blogosphere.API.Middlewares;
using Blogosphere.API.Models;
using Blogosphere.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogosphere.API.Controllers
{
   [ApiController]
   [Route("api/blogs")]
   public class BlogsController : ControllerBase
   {

      private readonly AppDbContext _context;

      public BlogsController(AppDbContext context)
      {
         _context = context;
      }

      [HttpGet]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<PagedResponse<Blog>>> Get(
         [FromQuery(Name = "category")] string? category,
         [FromQuery(Name = "page")] int? page
      )
      {
         if (!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }

         try
         {
            if (category == null || category == "")
            {
               List<Blog> b = await _context.Blogs
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
            var query = _context.Blogs.AsNoTracking();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var skipValue = (int)((page - 1) * PageSize);
            var blogs = await query
               .OrderByDescending(b => b.CreatedAt)
               .Skip(skipValue)
               .Take(PageSize)
               .ToListAsync();

            var metadata = new PaginationMetadata
            {
               CurrentPage = page ?? 1,
               PageSize = PageSize,
               TotalCount = totalCount,
               TotalPages = totalPages
            };

            return Ok(new PagedResponse<Blog>
            {
               Data = blogs,
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
      public async Task<ActionResult<Blog>> Get([FromRoute] int id)
      {
         try {
            var blog = await _context.Blogs
               .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null) {
               return NotFound();
            }

            return Ok(blog);
         }
         catch (Exception ex) {
            return Problem(
               detail: ex.Message,
               title: "An error occurred",
               statusCode: StatusCodes.Status500InternalServerError
            );
         }
      }

      [HttpPost]
      [RequireJwtValidation]
      public void Post([FromBody] string value)
      {
      }

      [HttpPut("{id}")]
      [RequireJwtValidation]
      public void Put(int id, [FromBody] string value)
      {
      }

      [HttpPatch("{id}")]
      [RequireJwtValidation]
      public void Patch(int id, [FromBody] string value)
      {
      }

      [HttpDelete("{id}")]
      [RequireJwtValidation]
      public void Delete(int id)
      {
      }
   }
}
