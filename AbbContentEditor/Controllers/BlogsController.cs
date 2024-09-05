using AbbContentEditor.Data;
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
        private readonly IBlogService _blogService;

        public BlogsController(IBlogService blogService, IMapper mapper)
        {
            _blogService = blogService;
            _mapper = mapper;
        }

        // GET: api/Blogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogListItem>>> GetBlogs()
        {
          //if (_context.Blogs == null)
          //{
          //    return NotFound();
          //}
          //List<Blog> result = await _context.Blogs.Include(b=>b.Category).ToListAsync();
          //  var resultDto = _mapper.Map<List<Blog>, IEnumerable<BlogListItem>>(result);
          //  Console.WriteLine(result);
          var us = _blogService.BlogRepository.GetAll();


            return Ok(us);
        }

        // GET: api/Blogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogListItemUser>> GetBlog(int id)
        {
          if (_blogService.BlogRepository == null)
          {
              return NotFound();
          }
            // var blog = _context.Blogs.Include(c => c.Category).FirstOrDefault(b => b.Id.Equals(id));
            var blog = _blogService.GetBlogById(id);
            if (blog == null)
            {
                return NotFound();
            }
            Console.WriteLine($"blog {blog.Title}");
            return Ok(_mapper.Map<Blog, BlogListItemUser>(blog) );
        }




        // POST: api/Blogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Blog>> PostBlog(Blog blog)
        {
            _blogService.BlogRepository.Add(blog);
            _blogService.SaveChanges();

            return CreatedAtAction("GetBlog", new { id = blog.Id }, blog);
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
                _blogService.BlogRepository.Update(blog);
                _blogService.SaveChanges();

                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!BlogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(ex);
                }
            }

            return NoContent();
        }

        private bool BlogExists(int id)
        {
            return _blogService.BlogRepository.GetById(id) != null;
        }


    }
}
