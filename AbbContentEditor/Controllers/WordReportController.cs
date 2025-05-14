using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordReportController : ControllerBase
    {
        private readonly IRepository<WordCollection> _wordColelctionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WordReportController> _logger;
        private readonly UserManager<AbbAppUser> _userManager;

        public WordReportController(IRepository<WordCollection> wordColelctionRepository, IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<WordReportController> logger, UserManager<AbbAppUser> userManager)
        {
            _wordColelctionRepository = wordColelctionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("wordreport")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null) return BadRequest();

            try
            {
                var r = _unitOfWork.wordHistoryRepository.Find(x => x.Where(x => x.IdentityUserId == user.Id), 1, 10).ToList();// Find(x => x.Where(x => x.Author.Id == t)); ;
                List<WordHistoryDto> results = _mapper.Map<List<WordHistoryDto>>(r);

                int correntResults = results.Count(x => x.Correct);
                int total = results.Count();
                DateTime minTime = results != null && results.Any() ? 
                        results.Min(x => x.AnswerTime) : DateTime.MinValue;

                DateTime maxTime = results != null && results.Any() ?
                        results.Max(x => x.AnswerTime) : DateTime.MinValue;
                
                var dateDiff = (maxTime - minTime).TotalHours;

                WordsReport wordsReport = new WordsReport()
                {
                    Attempts = total,
                    CorrectAnswers = correntResults,
                    TheTime = Math.Round(dateDiff, 2),
                    User = user.UserName
                };
                string strResult = JsonConvert.SerializeObject(results,Formatting.Indented  );
                string decodedText = Regex.Unescape(strResult);

                return Ok(wordsReport);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

    }


}
