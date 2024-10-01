using Blogosphere.API.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace Blogosphere.API.Controllers
{
    [ApiController]
    [Route("api/blogs")]
    public class BlogsController : ControllerBase
    {

        [HttpGet]
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
        [RequireJwtValidation]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpPatch("{id}")]
        [RequireJwtValidation]
        public void Patch(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        [RequireJwtValidation]
        public void Delete(int id)
        {
        }
    }
}
