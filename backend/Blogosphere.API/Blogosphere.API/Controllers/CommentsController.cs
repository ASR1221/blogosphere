using Blogosphere.API.Middlewares;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;
using Blogosphere.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
   [Route("api/comments")]
   [ApiController]
   public class CommentsController(ICommentsService service) : ControllerBase
   {
      private readonly ICommentsService _commentsService = service;

      [HttpGet("{blogId}")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<PagedResponse<Comment>>> Get([FromRoute] int blogId, [FromQuery(Name = "page")] int? page)
      {
         try
         {
            var comments = await _commentsService.Get(blogId, page);
            return Ok(comments);
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

      [HttpPost("{blogId}")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<Comment>> Add(
         [FromBody] AddCommentDto dto,
         [FromRoute] int blogId
      )
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

            string userId = (string)HttpContext.Items["UserId"];
            var comment = await _commentsService.Create(blogId, userId, dto);

            if (comment == null)
            {
               return NotFound();
            }

            return Ok(comment);
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

      [HttpPatch("{commentId}")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<Comment>> Patch(
         [FromRoute] int commentId,
         [FromBody] AddCommentDto dto
      )
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

            string userId = (string)HttpContext.Items["UserId"];
            var comment = await _commentsService.Update(commentId, userId, dto);

            if (comment == null)
            {
               return NotFound();
            }

            return Ok(comment);
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

      [HttpDelete("{commentId}")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<bool>> Delete([FromRoute] int commentId)
      {
         try
         {
            string userId = (string)HttpContext.Items["UserId"];
            var result = await _commentsService.Delete(commentId, userId);
            if (!result) return NotFound();
            return Ok(result);
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
