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
         [FromForm] IFormFile file
      ) {
         try
         {
            Console.WriteLine("FILE UPLOAD");
            if (file == null || file.Length == 0) {
               return BadRequest(new { error = "No file was provided" });
            }
            Console.WriteLine("FILE UPLOAD 2");

            var result = await _fileUploadService.UploadFileAsync(file);
            Console.WriteLine("FILE UPLOAD 3");

            return Ok(result);
         }
         catch (ArgumentException ex)
         {
            Console.WriteLine("FILE UPLOAD 4");

            return BadRequest(new { error = ex.Message });
         }
         catch (Exception ex)
         {
            Console.WriteLine("FILE UPLOAD 5");

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
         [FromForm] IFormFileCollection files
      ) {         
         try
         {
            var results = await _fileUploadService.UploadMultipleFilesAsync(files);
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
