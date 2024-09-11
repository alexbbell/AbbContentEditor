using AbbContentEditor.Data;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
      
        public BlogsController(IUnitOfWork unitOfWork, IMapper mapper)
            
        {
            _unitOfWork = unitOfWork;
            // _blogRepository = new Repository<Blog>(context);
            _mapper = mapper;
            // _appContext = context;
        }

        // GET: api/Blogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogListItem>>> GetBlogs([FromQuery]int page = 1, [FromQuery] int pageSize = 10)
        {

            var total = await _unitOfWork.blogRepository.GetAll().CountAsync();
            var result = await _unitOfWork.blogRepository.GetPaginatedStudentsWithCategory(page, pageSize).ToListAsync();
            var res = _mapper.Map<IEnumerable<Blog>, IEnumerable<BlogListItem>>(result);
            
          return Ok(res);
        }

        // GET: api/Blogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogListItemUser>> GetBlog(int id)
        {
            if (_unitOfWork.blogRepository == null)
            {
                return NotFound();
            }
            // var blog = _context.Blogs.Include(c => c.Category).FirstOrDefault(b => b.Id.Equals(id));
            var blog = await _unitOfWork.blogRepository.GetByIdAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            Console.WriteLine($"blog {blog.Title}");
            return Ok(_mapper.Map<Blog, BlogListItemUser>(blog));
        }




        // POST: api/Blogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Blog>> PostBlog(Blog blog)
        {
            _unitOfWork.blogRepository.AddAsync(blog);
            if(await _unitOfWork.Commit())
            {
                return CreatedAtAction("GetBlog", new { id = blog.Id }, blog);
            }
            else
            {
                return BadRequest();
            }
            //_unitOfWork.Commit();
        }


        // PUT: api/Blogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlog(int id, Blog blog)
        {
            if (id != blog.Id)
            {
                return BadRequest();
            }
            try
            {
                _unitOfWork.blogRepository.UpdateAsync(blog);
                _unitOfWork.Commit();

                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (await _unitOfWork.blogRepository.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(ex);
                }
            }


        }


        // DELETE: api/Blogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {

            var blog = await _unitOfWork.blogRepository.GetByIdAsync(id);
            if (blog == null) return NotFound();

            try
            {
                _ = _unitOfWork.blogRepository.DeleteAsync(blog);
                _ = _unitOfWork.Commit();
                return NoContent();
            }
            catch(Exception ex )
            {
                return BadRequest(ex.Message);
            }

        }

        //private bool BlogExists(int id)
        //{
        //    //return _blogRepository.GetById(id) != null;
        //}


    }
}
