using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AbbContentEditor.Data;
using AbbContentEditor.Models;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AutoMapper;

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

        public WordHistoriesController(IRepository<WordHistory> wordHistoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _wordHistoryRepository= wordHistoryRepository;
            _unitOfWork = unitOfWork;
            _mapper= mapper;
        }

        // GET: api/WordHistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WordHistory>>> GetWordHistories()
        {
            
            return Ok(_wordHistoryRepository.Find(x=>x.Where(x=>x.IdentityUserId == "1417a9c3-6e33-43c2-a02a-d692c8e0d335")));
        }

        // GET: api/WordHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WordHistory>> GetWordHistory(int id)
        {
            var wordHistory = await _wordHistoryRepository.GetByIdAsync(id);
            if (wordHistory == null)
            {
                return NotFound();
            }

            return wordHistory;
        }

        // PUT: api/WordHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWordHistory(int id, WordHistory wordHistory)
        {
            if (id != wordHistory.Id)
            {
                return BadRequest();
            }

            //_context.Entry(wordHistory).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!WordHistoryExists(id))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            return NoContent();
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWordHistory(int id)
        {
            //var wordHistory = await _context.WordHistories.FindAsync(id);
            //if (wordHistory == null)
            //{
            //    return NotFound();
            //}

            //_context.WordHistories.Remove(wordHistory);
            //await _context.SaveChangesAsync();
            throw new NotImplementedException();
            return NoContent();
        }

        private bool WordHistoryExists(int id)
        {
            throw new NotImplementedException();
            // return _context.WordHistories.Any(e => e.Id == id);
        }
    }
}
