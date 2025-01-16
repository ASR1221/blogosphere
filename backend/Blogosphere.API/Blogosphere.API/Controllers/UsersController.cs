using Blogosphere.API.Middlewares;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Models.Entities;
using Blogosphere.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
   [Route("api/users")]
   [ApiController]
   public class UsersController(IUsersService userService) : ControllerBase
   {
      private readonly IUsersService _userService = userService;

      [HttpPatch]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<User>> Patch([FromBody] EditUserDto editUserDto)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }
            var user = await _userService.Edit((string)HttpContext.Items["UserId"], editUserDto);

            if (user == null)
            {
               return NotFound();
            }

            return Ok(user);
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

      [HttpDelete]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<SuccessResponseDto>> Delete()
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return BadRequest();
            }

            var success = await _userService.Delete((string)HttpContext.Items["UserId"]);

            if (!success)
            {
               return NotFound();
            }

            return Ok(new SuccessResponseDto(
               Token: "",
               Message: "Blog Deleted Successfully"
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
