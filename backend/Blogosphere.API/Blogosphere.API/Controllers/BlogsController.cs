using Blogosphere.API.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {

        [HttpGet("forecast")] // This is a public endpoint (no auth needed, if auth needed use [HttpGet("secure")], and use [Authorize] when needed)
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IEnumerable<string> Get()
        {
            return [];
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(int id)
        {
            List<string> res = [];
            return Ok(res);
        }

        [HttpPost]
        [RequireJwtValidation]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpPatch("{id}")]
        public void Patch(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
