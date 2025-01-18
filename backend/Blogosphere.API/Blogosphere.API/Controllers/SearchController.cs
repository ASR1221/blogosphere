using Blogosphere.API.Models.Dtos;
using Blogosphere.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
   [Route("api/search")]
   [ApiController]
   public class SearchController(ISearchService service) : ControllerBase
   {

      private readonly ISearchService _searchService = service;

      [HttpGet]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status500InternalServerError)]
      public ActionResult<ActionResult<SearchResultDto>> Search(
         [FromQuery] string query,
         [FromQuery] string category,
         [FromQuery] int page,
         [FromQuery] int pageSize
      )
      {
         try
         {
            var result = _searchService.SmartSearch(query, category, page, pageSize);
            return Ok(result);
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
