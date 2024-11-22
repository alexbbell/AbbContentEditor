using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AbbContentEditor.Data;
using AbbContentEditor.Models;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountdownsController : ControllerBase
    {
        private readonly AbbAppContext _context;

        public CountdownsController(AbbAppContext context)
        {
            _context = context;
        }

        // GET: api/Countdowns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Countdown>>> GetCountdowns()
        {
            return await _context.Countdowns.ToListAsync();
        }

        // GET: api/Countdowns/5
        [HttpGet("{id}")]
        public async Task<ActionResult<int>> GetCountdown(Guid id)
        {
            var countdown = await _context.Countdowns.FindAsync(id);

            if (countdown == null)
            {
                return NotFound();
            }
            int result = (int)(countdown.EndTime - DateTime.Now).TotalSeconds;
            if (result < 0) { result = 0; }
            return result;
        }

        // PUT: api/Countdowns/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountdown(Guid id, Countdown countdown)
        {
            if (id != countdown.Id)
            {
                return BadRequest();
            }

            _context.Entry(countdown).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountdownExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countdowns
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Countdown>> PostCountdown(Countdown countdown)
        {
            _context.Countdowns.Add(countdown);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountdown", new { id = countdown.Id }, countdown);
        }

        // DELETE: api/Countdowns/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountdown(Guid id)
        {
            var countdown = await _context.Countdowns.FindAsync(id);
            if (countdown == null)
            {
                return NotFound();
            }

            _context.Countdowns.Remove(countdown);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountdownExists(Guid id)
        {
            return _context.Countdowns.Any(e => e.Id == id);
        }
    }
}
