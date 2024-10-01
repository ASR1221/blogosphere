using Blogosphere.API.Middlewares;
using Blogosphere.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
   [Route("api/auth")]
   [ApiController]
   public class AuthController : ControllerBase
   {
      private readonly IAuthService _authService;

      public AuthController(IAuthService authService)
      {
         _authService = authService;
      }

      [HttpPost("register")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto model)
      {
         try
         {
            var response = await _authService.Register(model);
            return Ok(response);
         }
         catch (Exception ex)
         {
            return Problem(detail: ex.Message, title: "An error occurred", statusCode: StatusCodes.Status500InternalServerError);
         }
      }

      [HttpPost("login")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto model)
      {
         try
         {
            var response = await _authService.Login(model);
            return Ok(response);
         }
         catch (UnauthorizedAccessException)
         {
            return Unauthorized();
         }
         catch (Exception ex)
         {
            return Problem(detail: ex.Message, title: "An error occurred", statusCode: StatusCodes.Status500InternalServerError);
         }
      }

      [HttpGet("refresh")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<AuthResponseDto>> Refresh()
      {
         try
         {
            if (HttpContext.Items.TryGetValue("UserId", out var userId))
            {
               var response = await _authService.Refresh(userId?.ToString() ?? "");
               return Ok(response);
            }

            return Unauthorized();
         }
         catch (Exception ex)
         {
            return Problem(detail: ex.Message, title: "An error occurred", statusCode: StatusCodes.Status500InternalServerError);
         }
      }
   }
}
