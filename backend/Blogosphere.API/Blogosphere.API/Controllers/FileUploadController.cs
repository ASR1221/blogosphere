using Microsoft.AspNetCore.Mvc;
using Blogosphere.API.Services;
using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Middlewares;

namespace Blogosphere.API.Controllers
{
   [Route("api/file")]
   [ApiController]
   public class FileUploadController : ControllerBase
   {
      private readonly IFileUploadService _fileUploadService;

      public FileUploadController(IFileUploadService fileUploadService)
      {
         _fileUploadService = fileUploadService;
      }

      [HttpPost("single")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<FileUploadDto>> UploadFile(
         IFormFile file,
         [FromQuery] string allowedExtensions = ".jpg,.jpeg,.png,.gif"
      ) {
         try
         {
            var result = await _fileUploadService.UploadFileAsync(file, allowedExtensions);
            return Ok(result);
         }
         catch (ArgumentException ex)
         {
            return BadRequest(new { error = ex.Message });
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

      [HttpPost("multiple")]
      [RequireJwtValidation]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<List<FileUploadDto>>> UploadMultipleFiles(
         [FromForm] IFormFileCollection files,
         [FromQuery] string allowedExtensions = ".jpg,.jpeg,.png,.gif"
      ) {
         try
         {
            var results = await _fileUploadService.UploadMultipleFilesAsync(files, allowedExtensions);
            return Ok(results);
         }
         catch (ArgumentException ex)
         {
            return BadRequest(new { error = ex.Message });
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
