using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
        public async Task<ActionResult<IEnumerable<WordHistoryDto>>> GetWordHistories()
        {

            string tempUserId = "e3326c3b-e8a3-481e-ad52-56a787695738";
            string tempUserId2 = "48c1bfc6-1bbb-4618-a87b-c40378cc31af";
            string email = "alexey@beliaeff.ru";
            var user = await _userManager.FindByIdAsync(tempUserId2);
            //var user = await _userManager.FindByEmailAsync(email.ToLower());
            var userId = user.Id;

            _logger.LogInformation($"User is {userId}");
            List<WordHistory> history = _wordHistoryRepository.Find(x => x.Where(x => x.IdentityUserId == userId)).ToList();
            List<WordHistoryDto> historyDto = _mapper.Map< List < WordHistory > , List <WordHistoryDto>>(history);
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
        [HttpPost]
        public async Task<ActionResult<WordHistoryDto>> PostWordHistory(WordHistoryDto wordHistory)
        {

            if (wordHistory == null) return BadRequest();

            var wh = _mapper.Map<WordHistoryDto, WordHistory>(wordHistory);
            
            await _unitOfWork.wordHistoryRepository.AddAsync(wh);
            if (await _unitOfWork.Commit()) return Ok(wh);
            return NoContent(); 

            //_context.WordHistories.Add(wordHistory);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetWordHistory", new { id = wordHistory.Id }, wordHistory);
            throw new NotImplementedException();
        }

        // DELETE: api/WordHistories/5
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteWordHistory(string userId)
        {
            //var items = _wordHistoryRepository.Find(x => x.Where(x => x.IdentityUserId == userId));

            int deletedCount = await _unitOfWork.wordHistoryRepository.DeleteAsync(x => x.Where(x => x.IdentityUserId == userId));
            try
            {
                await _unitOfWork.Commit();
                Console.WriteLine($"deletedCount: {deletedCount}");
                return Ok(deletedCount);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message.ToString()}");
                _logger.LogError($"Delete WordHistory error: {ex.Message}");
                return BadRequest(ex.Message );
            }

            //var wordHistory = await _context.WordHistories.FindAsync(id);
            //if (wordHistory == null)
            //{
            //    return NotFound();
            //}

            //_context.WordHistories.Remove(wordHistory);
            //await _context.SaveChangesAsync();

        }

        private bool WordHistoryExists(int id)
        {
            throw new NotImplementedException();
            // return _context.WordHistories.Any(e => e.Id == id);
        }
    }
}
