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
        public ActionResult<IEnumerable<Album>> GetAll()
        {
            var products = _repository.GetAll();
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
            _repository.Set(entities);
            return NoContent();
        }

        [HttpDelete]
        public IActionResult Delete(IEnumerable<int> ids)
        {
            _repository.Delete(ids);
            return NoContent();
        }
    }
}
