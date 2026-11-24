using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Controllers
{
    /// <summary>
    /// Tracks word-exercise sessions: start a session, record every answer (correct / wrong),
    /// finish it and get the statistics.
    ///
    ///   POST /api/ExerciseSessions                 start a session            -> ExerciseSessionDto
    ///   GET  /api/ExerciseSessions/active          current unfinished session -> ExerciseSessionDto | 204
    ///   POST /api/ExerciseSessions/{id}/answers    save one answer            -> ExerciseAnswerDto
    ///   POST /api/ExerciseSessions/{id}/finish     end the session            -> ExerciseSessionStatsDto
    ///   GET  /api/ExerciseSessions/{id}/stats      statistics of a session    -> ExerciseSessionStatsDto
    ///   GET  /api/ExerciseSessions                 past sessions (paged)      -> PagedItemResultDto&lt;ExerciseSessionSummaryDto&gt;
    ///   GET  /api/ExerciseSessions/overview        totals over all sessions   -> ExerciseOverviewDto
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExerciseSessionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ExerciseSessionsController> _logger;
        private readonly UserManager<AbbAppUser> _userManager;

        public ExerciseSessionsController(IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<ExerciseSessionsController> logger, UserManager<AbbAppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        // POST: api/ExerciseSessions
        [HttpPost]
        public async Task<ActionResult<ExerciseSessionDto>> StartSession([FromBody] StartExerciseSessionDto? request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            int? collectionId = request?.CollectionId;
            if (collectionId is > 0)
            {
                bool ownsCollection = await _unitOfWork.wordCollectionRepository.GetAll()
                    .AnyAsync(c => c.Id == collectionId && c.Author.Id == user.Id);
                if (!ownsCollection) return NotFound($"Word collection {collectionId} not found.");
            }
            else
            {
                collectionId = null;
            }

            // A user runs one exercise at a time: close any session left open (tab closed, crash, ...).
            var openSessions = await _unitOfWork.exerciseSessionRepository.GetAll()
                .Where(s => s.IdentityUserId == user.Id && s.FinishedAt == null)
                .Include(s => s.Answers)
                .ToListAsync();
            foreach (var open in openSessions)
            {
                open.FinishedAt = open.Answers.Count > 0 ? open.Answers.Max(a => a.AnswerTime) : open.StartedAt;
            }

            var session = new ExerciseSession
            {
                IdentityUserId = user.Id,
                WordCollectionId = collectionId,
                StartedAt = DateTime.UtcNow,
            };
            await _unitOfWork.exerciseSessionRepository.AddAsync(session);
            if (!await _unitOfWork.Commit()) return Problem("Could not start the exercise session.");

            var dto = _mapper.Map<ExerciseSessionDto>(session);
            return CreatedAtAction(nameof(GetStats), new { id = session.Id }, dto);
        }

        // GET: api/ExerciseSessions/active?collectionId=5
        [HttpGet("active")]
        public async Task<ActionResult<ExerciseSessionDto>> GetActiveSession([FromQuery] int? collectionId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var query = _unitOfWork.exerciseSessionRepository.GetAll()
                .Where(s => s.IdentityUserId == user.Id && s.FinishedAt == null);
            if (collectionId.HasValue) query = query.Where(s => s.WordCollectionId == collectionId);

            var session = await query.OrderByDescending(s => s.StartedAt).FirstOrDefaultAsync();
            if (session == null) return NoContent();
            return Ok(_mapper.Map<ExerciseSessionDto>(session));
        }

        // POST: api/ExerciseSessions/5/answers
        [HttpPost("{id:int}/answers")]
        public async Task<ActionResult<ExerciseAnswerDto>> SaveAnswer(int id, [FromBody] SaveAnswerDto answer)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var session = await FindOwnedSessionAsync(id, user.Id);
            if (session == null) return NotFound();
            if (session.FinishedAt != null) return Conflict("The exercise session is already finished.");

            var entry = new WordHistory
            {
                ExerciseSessionId = session.Id,
                IdentityUserId = user.Id,
                WordId = answer.WordId,
                Word = answer.Word,
                CorrectAnswer = answer.CorrectAnswer,
                SelectedAnswer = answer.SelectedAnswer,
                Correct = answer.Correct,
                AnswerTime = DateTime.UtcNow, // server time: the client clock is not trusted for durations
            };
            await _unitOfWork.wordHistoryRepository.AddAsync(entry);
            if (!await _unitOfWork.Commit()) return Problem("Could not save the answer.");

            return Created(string.Empty, _mapper.Map<ExerciseAnswerDto>(entry));
        }

        // POST: api/ExerciseSessions/5/finish
        // Idempotent: finishing an already finished session just returns its statistics.
        [HttpPost("{id:int}/finish")]
        public async Task<ActionResult<ExerciseSessionStatsDto>> FinishSession(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var session = await FindOwnedSessionAsync(id, user.Id, includeAnswers: true);
            if (session == null) return NotFound();

            if (session.FinishedAt == null)
            {
                session.FinishedAt = DateTime.UtcNow;
                if (!await _unitOfWork.Commit()) return Problem("Could not finish the exercise session.");
            }

            return Ok(BuildStats(session, user.UserName ?? string.Empty));
        }

        // GET: api/ExerciseSessions/5/stats
        [HttpGet("{id:int}/stats")]
        public async Task<ActionResult<ExerciseSessionStatsDto>> GetStats(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var session = await FindOwnedSessionAsync(id, user.Id, includeAnswers: true);
            if (session == null) return NotFound();

            return Ok(BuildStats(session, user.UserName ?? string.Empty));
        }

        // GET: api/ExerciseSessions?collectionId=5&pageNumber=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<PagedItemResultDto<ExerciseSessionSummaryDto>>> GetSessions(
            [FromQuery] int? collectionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _unitOfWork.exerciseSessionRepository.GetAll()
                .Where(s => s.IdentityUserId == user.Id);
            if (collectionId.HasValue) query = query.Where(s => s.WordCollectionId == collectionId);

            int total = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ExerciseSessionSummaryDto
                {
                    SessionId = s.Id,
                    CollectionId = s.WordCollectionId,
                    StartedAt = s.StartedAt,
                    FinishedAt = s.FinishedAt,
                    TotalAnswers = s.Answers.Count,
                    CorrectAnswers = s.Answers.Count(a => a.Correct),
                })
                .ToListAsync();

            foreach (var item in items)
            {
                item.WrongAnswers = item.TotalAnswers - item.CorrectAnswers;
                item.Accuracy = Accuracy(item.CorrectAnswers, item.TotalAnswers);
            }

            return Ok(new PagedItemResultDto<ExerciseSessionSummaryDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items,
            });
        }

        // GET: api/ExerciseSessions/overview?collectionId=5&top=10
        [HttpGet("overview")]
        public async Task<ActionResult<ExerciseOverviewDto>> GetOverview([FromQuery] int? collectionId, [FromQuery] int top = 10)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();
            top = Math.Clamp(top, 1, 50);

            var sessions = _unitOfWork.exerciseSessionRepository.GetAll()
                .Where(s => s.IdentityUserId == user.Id);
            if (collectionId.HasValue) sessions = sessions.Where(s => s.WordCollectionId == collectionId);

            var times = await sessions.Select(s => new { s.StartedAt, s.FinishedAt }).ToListAsync();

            var answers = _unitOfWork.wordHistoryRepository.GetAll()
                .Where(a => a.ExerciseSessionId != null && sessions.Any(s => s.Id == a.ExerciseSessionId));

            int total = await answers.CountAsync();
            int correct = await answers.CountAsync(a => a.Correct);

            var mistaken = await answers
                .GroupBy(a => a.WordId ?? a.Word)
                .Select(g => new WordStatDto
                {
                    WordId = g.Max(a => a.WordId),
                    Word = g.Max(a => a.Word)!,
                    CorrectAnswer = g.Max(a => a.CorrectAnswer),
                    Attempts = g.Count(),
                    Correct = g.Count(a => a.Correct),
                    Wrong = g.Count(a => !a.Correct),
                })
                .Where(w => w.Wrong > 0)
                .OrderByDescending(w => w.Wrong)
                .ThenBy(w => w.Word)
                .Take(top)
                .ToListAsync();

            return Ok(new ExerciseOverviewDto
            {
                CollectionId = collectionId,
                SessionCount = times.Count,
                TotalAnswers = total,
                CorrectAnswers = correct,
                WrongAnswers = total - correct,
                Accuracy = Accuracy(correct, total),
                TotalDurationSeconds = Math.Round(times
                    .Where(t => t.FinishedAt != null)
                    .Sum(t => Math.Max(0, (t.FinishedAt!.Value - t.StartedAt).TotalSeconds)), 1),
                LastSessionAt = times.Count > 0 ? times.Max(t => t.StartedAt) : null,
                MostMistakenWords = mistaken,
            });
        }

        // ---------- helpers ----------

        private async Task<AbbAppUser?> GetCurrentUserAsync()
        {
            var userName = User.GetUsername();
            return string.IsNullOrEmpty(userName) ? null : await _userManager.FindByNameAsync(userName);
        }

        private Task<ExerciseSession?> FindOwnedSessionAsync(int id, string userId, bool includeAnswers = false)
        {
            var query = _unitOfWork.exerciseSessionRepository.GetAll()
                .Where(s => s.Id == id && s.IdentityUserId == userId);
            if (includeAnswers) query = query.Include(s => s.Answers);
            return query.FirstOrDefaultAsync();
        }

        private static double Accuracy(int correct, int total) =>
            total == 0 ? 0 : Math.Round(correct * 100.0 / total, 1);

        internal static ExerciseSessionStatsDto BuildStats(ExerciseSession session, string userName)
        {
            var answers = session.Answers.OrderBy(a => a.AnswerTime).ThenBy(a => a.Id).ToList();
            int correct = answers.Count(a => a.Correct);
            DateTime end = session.FinishedAt ?? DateTime.UtcNow;

            var mistaken = answers
                .GroupBy(a => a.WordId ?? a.Word)
                .Select(g => new WordStatDto
                {
                    WordId = g.First().WordId,
                    Word = g.First().Word,
                    CorrectAnswer = g.Select(a => a.CorrectAnswer).FirstOrDefault(c => c != null),
                    Attempts = g.Count(),
                    Correct = g.Count(a => a.Correct),
                    Wrong = g.Count(a => !a.Correct),
                })
                .Where(w => w.Wrong > 0)
                .OrderByDescending(w => w.Wrong)
                .ThenBy(w => w.Word)
                .ToList();

            return new ExerciseSessionStatsDto
            {
                SessionId = session.Id,
                CollectionId = session.WordCollectionId,
                User = userName,
                StartedAt = session.StartedAt,
                FinishedAt = session.FinishedAt,
                IsFinished = session.FinishedAt != null,
                DurationSeconds = Math.Round(Math.Max(0, (end - session.StartedAt).TotalSeconds), 1),
                TotalAnswers = answers.Count,
                CorrectAnswers = correct,
                WrongAnswers = answers.Count - correct,
                Accuracy = Accuracy(correct, answers.Count),
                MistakenWords = mistaken,
                Answers = answers.Select(a => new ExerciseAnswerDto
                {
                    Id = a.Id,
                    WordId = a.WordId,
                    Word = a.Word,
                    CorrectAnswer = a.CorrectAnswer,
                    SelectedAnswer = a.SelectedAnswer,
                    Correct = a.Correct,
                    AnswerTime = a.AnswerTime,
                }).ToList(),
            };
        }
    }
}
