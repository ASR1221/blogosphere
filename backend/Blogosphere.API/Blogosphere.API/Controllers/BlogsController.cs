using Blogosphere.API.Middlewares;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
   [ApiController]
   [Route("api/blogs")]
   public class BlogsController : ControllerBase
   {

      private readonly IBlogsService _blogsService;

      public BlogsController(IBlogsService service)
      {
         _blogsService = service;
      }

      [HttpGet]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<PagedResponse<BlogInListResponseDto>>> Get(
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

            var response = await _blogsService.GetBlogs(category, page);

            return Ok(response);
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
      public async Task<ActionResult<SingleBlogResponseDto>> Get([FromRoute] int id)
      {
         try
         {
            var blog = await _blogsService.GetBlog(id, HttpContext);

            if (blog == null)
            {
               return NotFound();
            }
            
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

            await _blogsService.CreateBlog(body, (string)HttpContext.Items["UserId"]);

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

            var blog = await _blogsService.UpdateBlog(body, id, (string)HttpContext.Items["UserId"]);

            if (blog == null)
            {
               return NotFound();
            }

            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Blog Updated Successfully"
            ));

         }
         catch (UnauthorizedAccessException ex) {
            return Unauthorized(ex.Message);
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

            var success = await _blogsService.DeleteBlog(id, (string)HttpContext.Items["UserId"]);

            if (!success)
            {
               return NotFound();
            }

            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Blog Deleted Successfully"
            ));

         }
         catch (UnauthorizedAccessException ex) {
            return Unauthorized(ex.Message);
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
