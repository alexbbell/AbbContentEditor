using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordHistoriesController : ControllerBase
    {
        //private readonly AbbAppContext _context;
        private readonly IRepository<WordHistory> _wordHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WordHistoriesController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public WordHistoriesController(IRepository<WordHistory> wordHistoryRepository, 
            IUnitOfWork unitOfWork, IMapper mapper, ILogger<WordHistoriesController> logger, UserManager<IdentityUser> userManager)
        {
            _wordHistoryRepository= wordHistoryRepository;
            _unitOfWork = unitOfWork;
            _mapper= mapper;
            _logger = logger;
            _userManager = userManager;
    }

        // GET: api/WordHistories
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WordHistoryDto>>> GetWordHistories()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            //var user = await _userManager.FindByEmailAsync(email.ToLower());
            
            _logger.LogInformation($"User is {user.Id}");
            List<WordHistory> history = _wordHistoryRepository.Find(x => x.Where(x => x.IdentityUserId == user.Id)).ToList();
            List<WordHistoryDto> historyDto = _mapper.Map<List<WordHistory> , List <WordHistoryDto>>(history);
            return Ok(historyDto);
            //return Ok(_wordHistoryRepository.Find());
        }

        // GET: api/WordHistories/5
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<WordHistory>>> GetWordHistory(string userId)
        {
            var result = _wordHistoryRepository.Find(x => x.Where(x => x.IdentityUserId == userId));
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // PUT: api/WordHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWordHistory(int id, WordHistoryDto wordHistoryDto)
        {
            if (id != wordHistoryDto.Id)
            {
                return BadRequest();
            }
            var wh = _mapper.Map<WordHistoryDto, WordHistory>(wordHistoryDto);
            await _unitOfWork.wordHistoryRepository.UpdateAsync(wh);



            //_context.Entry(wordHistory).State = EntityState.Modified;

            try
            {
                await _unitOfWork.Commit();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WordHistoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(wh);
        }

        // POST: api/WordHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<WordHistoryDto>> PostWordHistory(WordHistoryDto wordHistory)
        {
            if (wordHistory == null) return BadRequest();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null) return BadRequest();


            var wh = _mapper.Map<WordHistoryDto, WordHistory>(wordHistory);
            var userId = await _userManager.FindByNameAsync(User.Identity.Name);

            wh.IdentityUserId = userId.Id;

            await _unitOfWork.wordHistoryRepository.AddAsync(wh);
            if (await _unitOfWork.Commit()) return Ok(wh);
            return NoContent();            
            
        }

        // DELETE: api/WordHistories/5
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteWordHistory()
        {
            //var items = _wordHistoryRepository.Find(x => x.Where(x => x.IdentityUserId == userId));

            var user = await _userManager.FindByNameAsync(userName: User.Identity.Name);
            if (user == null) return BadRequest();
            int deletedCount = await _unitOfWork.wordHistoryRepository.DeleteAsync(x => x.Where(x => x.IdentityUserId == user.Id));
            try
            {
                await _unitOfWork.Commit();
                Console.WriteLine($"deletedCount: {deletedCount}");
                return Ok(deletedCount.ToString());
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message.ToString()}");
                _logger.LogError($"Delete WordHistory error: {ex.Message}");
                return BadRequest(ex.Message );
            }

        }

        private bool WordHistoryExists(int id)
        {
            throw new NotImplementedException();
            // return _context.WordHistories.Any(e => e.Id == id);
        }
    }
}
