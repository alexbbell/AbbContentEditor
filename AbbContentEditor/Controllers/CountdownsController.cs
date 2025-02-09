using AbbContentEditor.Data;
using AbbContentEditor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountdownsController : ControllerBase
    {
        private readonly AbbAppContext _context;
        private ILogger<Countdown> _logger;

        public CountdownsController(AbbAppContext context, ILogger<Countdown> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Countdowns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Countdown>>> GetCountdowns()
        {
            return await _context.Countdowns.ToListAsync();
        }

        // GET: api/Countdowns/5
        [HttpGet("{username}")]
        public async Task<ActionResult<DateTime>> GetCountdown(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName.Equals(username));
            
            if (user == null)
            {
                _logger.LogError($"Non existing user {user}");
                return DateTime.UtcNow;
            }
            try
            {
                var existedCountdown = _context.Countdowns.FirstOrDefault(x => x.Id == new Guid(user.Id));
                DateTime result = existedCountdown.EndTime.ToLocalTime();
                return result;
            }
            catch (Exception ex)
            {
                return DateTime.MinValue;

            }
            //DateTime result = (existedCountdown.EndTime > DateTime.UtcNow) ? existedCountdown.EndTime: DateTime.MinValue;

        }

        // PUT: api/Countdowns/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountdown(Guid id, Countdown countdown)
        {
            if (id != countdown.Id)
            {
                return NoContent();
            }

            _context.Entry(countdown).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CountdownExists(id))
                {
                    _logger.LogError(ex.Message);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
                }
            }

            return NoContent();
        }

        // POST: api/Countdowns
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Countdown>> PostCountdown([FromBody]  CountDownRequest data)
        {
            var username = data.UserName;
            var user = _context.Users.FirstOrDefault(u => u.UserName.Equals(username));
            if(user == null)  return BadRequest();

            var existedCountdown = _context.Countdowns.FirstOrDefault(x=>x.Id == new Guid(user.Id));
            if (existedCountdown == null)
            {
                existedCountdown = new Countdown { Id = new Guid(user.Id), Name = username, CreatedAt = DateTime.UtcNow, 
                    EndTime = (data.Action == "start") ? DateTime.UtcNow.AddSeconds(10) : DateTime.MinValue
                };
                _context.Countdowns.Add(existedCountdown );
            }
            else
            {
                existedCountdown.EndTime = (data.Action == "start") ? DateTime.UtcNow.AddSeconds(10) : DateTime.MinValue;
                _context.Countdowns.Update(existedCountdown);
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountdown", new { username = username }, existedCountdown);
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
