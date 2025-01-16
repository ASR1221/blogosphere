using Blogosphere.API.Middlewares;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
   [Route("api/blogs/{blogId}/likes")]
   [ApiController]
   public class LikesController : ControllerBase
   {
      ILikesService _likesService;

      public LikesController(ILikesService likesService) => _likesService = likesService;

      [HttpPost]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<SuccessResponseDto>> AddLike([FromRoute] int blogId)
      {
         try
         {
            string userId = (string)HttpContext.Items["UserId"];
            var result = await _likesService.Add(blogId, userId);
            if (!result) return NotFound();
            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Like Added Successfully"
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

      [HttpDelete("{likeId}")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<SuccessResponseDto>> RemoveLike([FromRoute] int likeId)
      {
         try
         {
            string userId = (string)HttpContext.Items["UserId"];
            var result = await _likesService.Delete(likeId, userId);
            if (!result) return NotFound();
            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Like Removed Successfully"
            ));
         }
         catch (UnauthorizedAccessException ex)
         {
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
