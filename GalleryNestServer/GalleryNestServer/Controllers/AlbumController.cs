using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GalleryNestServer.Controllers
{
    [Route("api/album")]
    [ApiController]
    public class AlbumController(EntityRepository<Album> repository) : ControllerBase
    {
        private readonly EntityRepository<Album> _repository = repository;

        [HttpGet]
        public ActionResult<IEnumerable<Album>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var products = _repository.GetPaged(pageNumber, pageSize);
            return Ok(products);
        }
        [HttpGet("{id}")]
        public ActionResult<Album> GetById([FromQuery] int id)
        {
            var products = _repository.GetById(id);
            return Ok(products);
        }

        [HttpPost]
        public IActionResult Set([FromBody] IEnumerable<Album> entities)
        {
            if (entities.Count() < 0) return BadRequest();
            _repository.Set(entities);
            return NoContent();
        }

        [HttpDelete]
        public IActionResult Delete([FromQuery] IEnumerable<int> ids)
        {
            if (ids.Count() < 0) return BadRequest();
            _repository.Delete(ids);
            return NoContent();
        }
    }
}
