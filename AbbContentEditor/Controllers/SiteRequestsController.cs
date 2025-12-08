using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteRequestsController : ControllerBase
    {
        private readonly ILogger<SiteRequestsController> _logger;
        private readonly AbbAppContext _context;
        private readonly IRepository<SiteRequest> _repository;
        private readonly IUnitOfWork _unitOfWork;
        public SiteRequestsController(ILogger<SiteRequestsController> logger, AbbAppContext context,
            IRepository<SiteRequest> repository, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _context = context;
            _repository = repository;
            _unitOfWork = unitOfWork;

        }
        [HttpPost]
        [Route("getitems")]
#if !DEBUG
        [Authorize]
#endif
        public async Task<IActionResult> GetRequests(int page, int pagesize)
        {
            page = (page > 0) ? page : 1;

            pagesize = (pagesize > 0) ? pagesize : 10;

            var items = _repository.FetchItems(pageNumber: page, pageSize: pagesize, orderBy: x => x.Created, descending: true);
            PagedItemResultDto<SiteRequest> result = new PagedItemResultDto<SiteRequest>
            {
                Items = items.Query.ToList(),
                PageNumber = page,
                PageSize = pagesize,
                TotalCount = items.TotalCount

            };

            return Ok(result);
        }


        [HttpGet]
        [Route("{id}")]
#if !DEBUG
        [Authorize]
#endif
        public async Task<IActionResult> GetRequest(int id)
        {

            var item = _repository.Find(x => x.Where(x => x.Id == id)).FirstOrDefault();
            if (item == null) return NotFound();
            SiteRequestDto result = new SiteRequestDto
            {
                Id = item.Id,
                Email = item.Email,
                Subject = item.Subject,
                Question = item.Question,
                TheName = item.TheName,
                Created = item.Created
            };
            return Ok(result);
        }

        [HttpPost]
        [EnableRateLimiting("contact-limit")]
        public async Task<IActionResult> Post([FromBody] SiteRequestDto request)
        {
            if (request == null) return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                SiteRequest sr = new SiteRequest()
                {
                    Email = request.Email,
                    Question = request.Question,
                    Subject = request.Subject,
                    Status = "New",
                    TheName = request.TheName,
                    Created = request.Created
                };
                _context.SiteRequests.Add(sr);
                _context.SaveChanges();
                return Ok(JsonSerializer.Serialize(sr));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error on saving the request. {ex.Message}");
                return BadRequest(ex.Message);
            }
        }


        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateStasus([FromRoute] int id, [FromBody] SiteRequestDto request)
        {
            if (request == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var item = _unitOfWork.siteRequestsRepository.Find(x => x.Where(su => su.Id == request.Id)).FirstOrDefault();
            if (item == null) return NotFound();
            if (request.Id != item.Id) return BadRequest(request.Id);

            try
            {
                item.Email = request.Email;
                item.Question = request.Question;
                item.Subject = request.Subject;
                item.Subject = request.Subject;
                item.TheName = request.TheName;
                await _unitOfWork.siteRequestsRepository.UpdateAsync(item);
                await _unitOfWork.Commit();
                return Ok(JsonSerializer.Serialize(item));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error on updating the Site request. {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteItem([FromRoute] int id)
        {
            if (id == 0) return BadRequest();

            var item = await _unitOfWork.siteRequestsRepository.GetByIdAsync(id);
            if (item == null) return NotFound();
            await _unitOfWork.siteRequestsRepository.DeleteAsync(item);
            await _unitOfWork.Commit();
            return Ok(JsonSerializer.Serialize(item));

        }
    }
}
