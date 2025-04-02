using AbbContentEditor.Data.UoW;
using AbbContentEditor.Helpers;
using AbbContentEditor.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AbbContentEditor.Controllers
{
    public class MoveFilesRequest
    {
        public string[] Files { get; set; }
        public int PostId { get; set; }
    }


    public class MoveFileRequest
    {
        public string File { get; set; }
        public int PostId { get; set; }
    }
    
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class BlogsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private string _mainUploadDir;
        private ILogger _logger;
        private ImageUtilities _imageUtilities;

        public BlogsController(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, 
            ILogger<BlogsController> logger, ImageUtilities imageUtilities)
            
        {
            _unitOfWork = unitOfWork;
            // _blogRepository = new Repository<Blog>(context);
            _mapper = mapper;
            // _appContext = context;
            _mainUploadDir = configuration.GetSection("UploadFolder").Value;
            _logger = logger;
            _imageUtilities = imageUtilities;
        }

        // GET: api/Blogs
        [HttpGet]

        public async Task<ActionResult<BlogListItemsResponse>> GetBlogs([FromQuery]int page = 1, [FromQuery] int pageSize = 10)
        {

            var total = await _unitOfWork.blogRepository.GetAll().CountAsync();
            var result = await _unitOfWork.blogRepository.GetPaginatedBlogsWithCategory(page, pageSize).ToListAsync();
            var res = _mapper.Map<IEnumerable<Blog>, IEnumerable<BlogListItem>>(result);

            BlogListItemsResponse blogListItemsResponse = new BlogListItemsResponse();
            blogListItemsResponse.Total = total;
            blogListItemsResponse.BlogItems = res;

          return Ok(blogListItemsResponse);
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
            var blog = _unitOfWork.blogRepository.GetBlogListItem(id);
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
            try
            {
                blog.UpdDate = DateTime.UtcNow;
                _unitOfWork.blogRepository.AddAsync(blog);
                await _unitOfWork.Commit();
                _logger.LogInformation($"Post is added.");
                //return CreatedAtAction("GetBlog", new { id = blog.Id }, blog);
                return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blog);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Error on adding the post. {ex}");
                return BadRequest(ex.Message);
            }
        }


        // PUT: api/Blogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlog(int id, Blog blog)
        {
            if (id != blog.Id)
            {
                _logger.LogError($"Error on updating the post. Post {id} not found");

                return BadRequest();
            }
            try
            {
                blog.UpdDate = DateTime.UtcNow;
                _unitOfWork.blogRepository.UpdateAsync(blog);
                _unitOfWork.Commit();

                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (await _unitOfWork.blogRepository.GetByIdAsync(id) == null)
                {
                    _logger.LogError($"Error on updating the post. Error on save {id}. {ex}");
                    return NotFound();
                }
                else
                {
                    _logger.LogError($"Error on updating the post. {ex}");
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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            string _imageDirectory = Path.Combine(_mainUploadDir, "UploadedImagesTMP");
            if (!Directory.Exists(_imageDirectory))
            {
                Directory.CreateDirectory(_imageDirectory);
            }
            try
            {
                // Get the original file name and append a unique identifier to avoid collisions
                // TODO: When I implement authentication, add a directory with GUID name as user directory
                // var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var fileName = $"{file.FileName}";
                // Define the full path for the file on the server
                var filePath = Path.Combine(_imageDirectory, fileName);
                // Save the file to the specified directory
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok(new { Message = "File uploaded successfully.", FileName = fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("moveImages")]
        public async Task<IActionResult> MoveImages([FromBody] MoveFilesRequest request)
        {
            var files = request.Files;
            var postId = request.PostId;            
            if (files == null || files.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            List<OperationResult>  images = _imageUtilities.MoveFiles(request);

            if (images == null || images.Count == 0) return BadRequest($"Error on Moving photos");

            var jsonImages = JsonSerializer.Serialize(images, JsonSerializerOptions.Default);
            return Ok(new { Message = $"{images.Count()}  of {request.Files.Count()} uploaded successfully. {jsonImages}"});
        }

        [HttpPost("deleteImage")]
        public async Task<bool>DeleteImages([FromBody]  MoveFileRequest request)
        {
            int postId= request.PostId;
            string filename = request.File;
            /// Here we dermine if we remove a file from the temp directory or from the existing blogId dir
            string directory = (postId != 0) ? Path.Combine(_mainUploadDir, "UploadedImagesTMP") : Path.Combine(_mainUploadDir, "uploads", postId.ToString());
            return _imageUtilities.RemoveFile(directory, filename);
        }

        //private bool BlogExists(int id)
        //{
        //    //return _blogRepository.GetById(id) != null;
        //}


    }
}
