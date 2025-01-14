using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CategoriesController(IRepository<Category> categoryRepository, 
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        // GET: api/<CategoriesController>
        [HttpGet]
        public IEnumerable<Category> GetCategory()
        {
            return _categoryRepository.GetAll();
        }

        // GET api/<CategoriesController>/5
        [HttpGet("{id}")]
        public Task<Category> GetCategory(int id)
        {
            var res = _categoryRepository.GetByIdAsync(id);
            return res;
        }

        // POST api/<CategoriesController>
        [HttpPost]
        public async Task<ActionResult<Category>> Post(Category category)
        {
            //throw new NotImplementedException();

            _unitOfWork.categoryRepository.AddAsync(category);
            if (await _unitOfWork.Commit()) return CreatedAtAction("GetCategory", new { id = category.Id }, category);
            
            return BadRequest();
            
        }

        // PUT api/<CategoriesController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> Put(int id, Category category)
        {
            if (id != category.Id) return BadRequest();
            
            var item = await _unitOfWork.categoryRepository.GetByIdAsync(id);
            if (item == null) return BadRequest();

            item.Name = category.Name;
            _unitOfWork.categoryRepository.UpdateAsync(item);
            if (await _unitOfWork.Commit()) return Ok(item);
            return BadRequest();
        }

        // DELETE api/<CategoriesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
